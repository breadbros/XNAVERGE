using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Meta;
using JSIL.Proxy;

namespace Sully.Proxies {
    [JSProxy(
        "Newtonsoft.Json.JsonConvert",
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared,
        inheritable: true
    )]
    public static class NewtonsoftJSONProxy {
        [JSReplacement("JSIL.JSON.Parse($value)")]
        [JSIsPure]
        public static T DeserializeObject<T> (string value, params AnyType[] converters) {
            throw new InvalidOperationException();
        }
    }
}
