namespace Ode.Domain.Engine.JsonSerialization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Serialization;

    public class SnapshotSerialization : ISerializeSnapshots
    {
        private readonly JsonSerializerSettings jsonSerializationSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new SnapshotContractResolver(),
            Formatting = Formatting.None
        };

        public TSnapshot DeserializeSnapshot<TSnapshot>(string serializedSnapshot)
        {
            return JsonConvert.DeserializeObject<TSnapshot>(serializedSnapshot, this.jsonSerializationSettings);
        }

        public string SerializeSnapshot<TSnapshot>(TSnapshot snapshotToSerialize)
        {
            return JsonConvert.SerializeObject(snapshotToSerialize, this.jsonSerializationSettings);
        }
    }

    internal class SnapshotContractResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var memberInfo = base.GetSerializableMembers(objectType);

            memberInfo.AddRange(objectType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.MemberType == MemberTypes.Field));

            return memberInfo;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var jsonProperties = base.CreateProperties(type, memberSerialization);

            foreach (var jsonProperty in jsonProperties)
            {
                jsonProperty.Readable = true;
                jsonProperty.Writable = true;
            }

            return jsonProperties;
        }
    }
}