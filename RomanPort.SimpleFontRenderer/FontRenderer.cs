using System;

namespace RomanPort.SimpleFontRenderer
{
    public unsafe class FontRenderer
    {
        public FontRenderer(int imageWidth, int imageHeight, IFontRendererPixelFormat format)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.format = format;
        }

        private int imageWidth;
        private int imageHeight;
        private IFontRendererPixelFormat format;

        private void SetMixColor(byte* ptr, float ratio, FontColor foreground, FontColor background)
        {
            switch(ratio)
            {
                case 0:
                    format.WritePixel(ptr, background.r, background.g, background.b);
                    break;
                case 1:
                    format.WritePixel(ptr, foreground.r, foreground.g, foreground.b);
                    break;
                default:
                    float ratioInvert = 1 - ratio;
                    format.WritePixel(ptr,
                        (foreground.r * ratio) + (background.r * ratioInvert),
                        (foreground.g * ratio) + (background.g * ratioInvert),
                        (foreground.b * ratio) + (background.b * ratioInvert)
                    );
                    break;
            }
        }

        public byte* GetOffsetPixel(void* ptr, int x, int y)
        {
            return (byte*)ptr + (((y * imageWidth) + x) * format.BytesPerPixel);
        }

        public void RenderCharacter(byte* ptr, char c, FontPack font, FontColor foreground, FontColor background)
        {
            //Make sure we have this
            if (!font.font.ContainsKey(c))
                return;

            //Get lines
            float[] data = font.font[c];

            //Copy
            for (int y = 0; y < font.height; y++)
            {
                //Get pointer and offset
                byte* line = ptr + (y * format.BytesPerPixel * imageWidth);
                int offset = font.width * y;

                //Mix
                for (int x = 0; x < font.width; x++)
                {
                    SetMixColor(line, data[offset + x], foreground, background);
                    line += format.BytesPerPixel;
                }
            }
        }

        public bool RenderRawBox(byte* ptr, char[][] lines, FontPack font, FontAlignHorizontal horizTextAlign, FontAlignVertical vertTextAlign, int boundsWidth, int boundsHeight, FontColor foreground, FontColor background)
        {
            //Fill box with background
            for(int y = 0; y<boundsHeight; y++)
            {
                byte* fillLine = ptr + (format.BytesPerPixel * y * imageWidth);
                for(int x = 0; x<boundsWidth; x++)
                {
                    format.WritePixel(fillLine, background.r, background.g, background.b);
                    fillLine += format.BytesPerPixel;
                }
            }
            
            //Determine Y offset
            int offsetY;
            switch (vertTextAlign)
            {
                case FontAlignVertical.Top: offsetY = 0; break;
                case FontAlignVertical.Bottom: offsetY = boundsHeight - (lines.Length * font.height); break;
                case FontAlignVertical.Center: offsetY = (boundsHeight - (lines.Length * font.height)) / 2; break;
                default: throw new Exception("Unknown alignment.");
            }

            //Draw
            bool fitAll = true;
            byte* line = ptr + (format.BytesPerPixel * offsetY * imageWidth);
            for (int ln = 0; ln < lines.Length; ln++)
            {
                //Calculate X offset
                int offsetX;
                switch (horizTextAlign)
                {
                    case FontAlignHorizontal.Left: offsetX = 0; break;
                    case FontAlignHorizontal.Right: offsetX = boundsWidth - (lines[ln].Length * font.width); break;
                    case FontAlignHorizontal.Center: offsetX = (boundsWidth - (lines[ln].Length * font.width)) / 2; break;
                    default: throw new Exception("Unknown alignment.");
                }

                //Write
                byte* lineDraw = line + (offsetX * format.BytesPerPixel);
                byte* maxDraw = line + ((boundsWidth - font.width) * format.BytesPerPixel);
                for (int i = 0; i < lines[ln].Length && lineDraw <= maxDraw; i++)
                {
                    RenderCharacter(lineDraw, lines[ln][i], font, foreground, background);
                    lineDraw += format.BytesPerPixel * font.width;
                }

                //Update flag if we have too much
                fitAll = fitAll && (lineDraw <= maxDraw);

                //Offset
                line += format.BytesPerPixel * imageWidth * font.height;
            }

            return fitAll;
        }

    }
}
