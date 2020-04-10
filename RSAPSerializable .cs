using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;

/// <summary>
/// RSAParameters are not serializable and so another object 'RSAPSerializable' has been made in order to fascilitate serialization of RSAParameters so that Keys can be saved to files.
/// </summary>
[Serializable]
public class RSAPSerializable : ISerializable
{
    private RSAParameters _rsaParameters;

    public RSAParameters RSAParameters
    {
        get
        {
            return _rsaParameters;
        }
    }

    public RSAPSerializable(RSAParameters rsaParameters)
    {
        _rsaParameters = rsaParameters;
    }

    private RSAPSerializable()
    {
    }

    public byte[] D { get { return _rsaParameters.D; } set { _rsaParameters.D = value; } }

    public byte[] DP { get { return _rsaParameters.DP; } set { _rsaParameters.DP = value; } }

    public byte[] DQ { get { return _rsaParameters.DQ; } set { _rsaParameters.DQ = value; } }

    public byte[] Exponent { get { return _rsaParameters.Exponent; } set { _rsaParameters.Exponent = value; } }

    public byte[] InverseQ { get { return _rsaParameters.InverseQ; } set { _rsaParameters.InverseQ = value; } }

    public byte[] Modulus { get { return _rsaParameters.Modulus; } set { _rsaParameters.Modulus = value; } }

    public byte[] P { get { return _rsaParameters.P; } set { _rsaParameters.P = value; } }

    public byte[] Q { get { return _rsaParameters.Q; } set { _rsaParameters.Q = value; } }

    /// <summary>
    /// Necessary for serialization, the GetObjectData method specifies which values are to be loaded during serialization and deserialization.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("D", _rsaParameters.D);
        info.AddValue("DP", _rsaParameters.DP);
        info.AddValue("DQ", _rsaParameters.DQ);
        info.AddValue("Exponent", _rsaParameters.Exponent);
        info.AddValue("InverseQ", _rsaParameters.InverseQ);
        info.AddValue("Modulus", _rsaParameters.Modulus);
        info.AddValue("P", _rsaParameters.P);
        info.AddValue("Q", _rsaParameters.Q);
    }
}