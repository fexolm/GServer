using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local

namespace GServer.Containers
{
    public class DsSerializeAttribute : Attribute
    {
        public readonly SerializationOptions Options;

        [Flags]
        public enum SerializationOptions
        {
            None,
            Optional,
        }

        public DsSerializeAttribute() {
            Options = SerializationOptions.None;
        }

        public DsSerializeAttribute(SerializationOptions props) {
            Options = props;
        }
    }
}