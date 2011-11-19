using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = System.IO.MemoryStream;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentImporter(".vsp", DisplayName = "VERGE VSP Importer", DefaultProcessor = "VSPProcessor")]
    public class VSPImporter : ContentImporter<TImport> {
        public override TImport Import(string filename, ContentImporterContext context) {
            FileStream fs;
            MemoryStream ms = new MemoryStream();
            fs = null;
            try {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                fs.CopyTo(ms);
            }
            finally {
                if (fs != null) fs.Close();
            }
            ms.Position = 0;
            
            return ms;
        }
    }
}
