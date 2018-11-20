using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.Finalization
{
    /* Listing 12-3 */
    class MemoryAwareBitmap
    {
        private System.Drawing.Bitmap bitmap;
        private long memoryPressure;

        public MemoryAwareBitmap(string file, long size)
        {
            bitmap = new System.Drawing.Bitmap(file);
            if (bitmap != null)
            {
                memoryPressure = size;
                GC.AddMemoryPressure(memoryPressure);
            }
        }

        ~MemoryAwareBitmap()
        {
            if (bitmap != null)
            {
                bitmap.Dispose();
                GC.RemoveMemoryPressure(memoryPressure);
            }
        }
    }
}
