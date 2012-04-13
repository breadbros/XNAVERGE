using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Meta;
using JSIL.Proxy;

namespace Sully.Proxies {
    [JSProxy(
        "XNAVERGE.Utility",
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared,
        inheritable: true
    )]
    public static class UtilityProxy {
        [JSReplacement("System.IO.File.ReadAllText($filepath)")]
        [JSIsPure]
        public static String read_file_text (String filepath) {
            throw new NotImplementedException();
        }
    }
}
