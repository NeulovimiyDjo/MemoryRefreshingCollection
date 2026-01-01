using System;
using System.Collections.Generic;
using System.Linq;
using ES = ExecuteStrategy;

namespace MyXmlSerializerProject
{
    public static class ClassesSettings
    {
        public static Dictionary<Type, IClassSettings> Settings => new List<IClassSettings>()
        {
            new ClassSettings<MyTestType>(new()
                {
                    { x => x.Prop1.InnerProp1, new(ES.Strat1) },
                    { x => x.Prop1.InnerProp2.InnerInnerProp1, new(ES.Strat2) },
                    { x => x.Prop1.InnerProp2.InnerInnerProp2[0].LeafProp1, new(ES.Strat1) },
                    { x => x.Prop2[0].InnerProp1[0].InnerInnerProp1[0].LeafProp1, new(ES.Strat3) },
                }
            ),
        }.ToDictionary(x => x.GetType().GetGenericArguments().Single(), x => x);
    }
}
