using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Meta;
using JSIL.Proxy;
using fastJSON;

namespace Sully.Proxies {
    [JSProxy(
        typeof(fastJSON.JSON),
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared
    )]
    public class FastJSONProxy {
        [JSReplacement("JSIL.JSON.Parse($json)")]
        [JSIsPure]
        public object Parse (string json) {
            throw new InvalidOperationException();
        }
    }
}
