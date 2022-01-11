/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Expression
    {
        public object[] Elements { get; private set; }

        public Expression(params object[] elements)
        {
            /*
             * var e = elements.FirstOrDefault(elt => elt != null && !(elt is string) && !(elt is IList) && elt is IEnumerable);
            if (e != null) {

                throw new System.Exception(e.ToString());
            }
            */

            Elements = elements;
        }

        /*public Expression(IEnumerable elements)
        {
            var elementsAsList = new List<object>();
            foreach (var element in elements)
            {
                elementsAsList.Add(element);
            }
            
            Elements = elementsAsList.ToArray();
        }*/

        public Expression(List<object> elements)
        {
            Elements = elements.ToArray();
        }
    }

}
