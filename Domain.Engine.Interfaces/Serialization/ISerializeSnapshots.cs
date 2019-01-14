namespace Ode.Domain.Engine.Serialization
{
    public interface ISerializeSnapshots
    {
        string SerializeSnapshot<TSnapshot>(TSnapshot snapshotToSerialize);

        TSnapshot DeserializeSnapshot<TSnapshot>(string serializedSnapshot);
    }
}
