﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using Signal_Block_Design_Tool.Files;

namespace Signal_Block_Design_Tool.Text
{
    public class TextWriter
    {
        private readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 10);
        private readonly Bitmap TextBitmap;
        private List<PointF> _positions;
        private List<string> _lines;
        private List<Brush> _colors;
        private int _textureId;
        private Size _clientSize;

        public void Update(int ind, string newText)
        {
            if (ind < _lines.Count)
            {
                _lines[ind] = newText;
                UpdateText();
            }
        }

        public TextWriter(Size clientSize, Size areaSize)
        {
            _positions = new List<PointF>();
            _lines = new List<string>();
            _colors = new List<Brush>();

            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            _clientSize = clientSize;
            _textureId = CreateTexture();
        }

        private int CreateTexture()
        {
            int textureId;
            OpenTK.Graphics.GL.TexEnv(OpenTK.Graphics.TextureEnvTarget.TextureEnv, OpenTK.Graphics.TextureEnvParameter.TextureEnvMode, (float)OpenTK.Graphics.TextureEnvMode.Replace);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            OpenTK.Graphics.GL.GenTextures(1, out textureId);
            OpenTK.Graphics.GL.BindTexture(OpenTK.Graphics.TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            OpenTK.Graphics.GL.TexImage2D(OpenTK.Graphics.TextureTarget.Texture2D, 0, OpenTK.Graphics.PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.PixelFormat.Bgra, OpenTK.Graphics.PixelType.UnsignedByte, data.Scan0);
            OpenTK.Graphics.GL.TexParameter(OpenTK.Graphics.TextureTarget.Texture2D, OpenTK.Graphics.TextureParameterName.TextureMinFilter, (int)OpenTK.Graphics.TextureMinFilter.Linear);
            OpenTK.Graphics.GL.TexParameter(OpenTK.Graphics.TextureTarget.Texture2D, OpenTK.Graphics.TextureParameterName.TextureMagFilter, (int)OpenTK.Graphics.TextureMagFilter.Linear);

            OpenTK.Graphics.GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;

        }

        public void Dispose()
        {
            if (_textureId > 0)
            {
                GL.DeleteTexture(_textureId);
            }
        }

        public void Clear()
        {
            _lines.Clear();
            _positions.Clear();
            _colors.Clear();
        }

        public void AddLine(string text, PointF position, Brush color)
        {
            _lines.Add(text);
            _positions.Add(position);
            _colors.Add(color);
            UpdateText();

        }

        public void UpdateText()
        {
            if (_lines.Count > 0)
            {
                using (Graphics graphics = Graphics.FromImage(TextBitmap))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        graphics.DrawString(_lines[i], TextFont, _colors[i], _positions[i]);
                    }
                }
            }

            System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            OpenTK.Graphics.GL.TexSubImage2D(OpenTK.Graphics.TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.PixelFormat.Bgra, OpenTK.Graphics.PixelType.UnsignedByte, data.Scan0);
            TextBitmap.UnlockBits(data);
        }

        public void Draw(Camera2D camera)
        {
            GL.PushMatrix();
            GL.LoadIdentity();

            Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, _clientSize.Width, _clientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);

            //Matrix4 projection = camera.getTransformation();
            //OpenTK.Graphics.GL.MatrixMode(OpenTK.Graphics.MatrixMode.Projection);

            GL.PushMatrix();
            GL.LoadMatrix(ref ortho_projection);


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstColor);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);


            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex2(TextBitmap.Width, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex2(TextBitmap.Width, TextBitmap.Height);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, TextBitmap.Height);
            GL.End();
            GL.PopMatrix();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();


        }
    }
}
