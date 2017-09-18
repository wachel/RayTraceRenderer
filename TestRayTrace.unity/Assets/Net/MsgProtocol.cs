using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum MsgID
{
    Test,
    Num,
}

public abstract class MsgBase
{
    public abstract MsgID ID { get; }
    public abstract void Pack(BinaryWriter writer);
    public abstract void Unpack(BinaryReader reader);
    public virtual MsgBase Clone() { return null; }
}

public class MsgTest : MsgBase
{
    int width;
    int height;
    public override MsgID ID { get { return MsgID.Test; } }
    public override void Pack(BinaryWriter writer)
    {
        writer.Write(width);
        writer.Write(height);
    }
    public override void Unpack(BinaryReader reader)
    {
        width = reader.ReadInt32();
        height = reader.ReadInt32();
    }
}

public static class MessageFactory
{
    public static MsgBase Create(MsgID msgID)
    {
        switch (msgID) {
            case MsgID.Test:return new MsgTest();
            default:return null;
        }
    }
}

public static class MessagePacker
{
    static MemoryStream stream = new MemoryStream();
    static BinaryWriter writer = new BinaryWriter(stream);

    public static byte[] Pack(MsgBase msg)
    {
        stream.SetLength(0);

        writer.Seek(4, SeekOrigin.Begin);
        writer.Write((int)msg.ID);
        msg.Pack(writer);
        writer.Seek(0, SeekOrigin.Begin);
        writer.Write((int)stream.Length - 4);
        return stream.GetBuffer();
    }
}

public static class MessageUnpacker
{
    public static MsgBase Unpack(byte[] data)
    {
        MemoryStream bodyStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(bodyStream);
        MsgID msgID = (MsgID)reader.ReadInt32();
        MsgBase msg = MessageFactory.Create(msgID);
        msg.Unpack(reader);
        return msg;
    }
}