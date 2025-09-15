using System;
using System.Numerics;
using System.Security.Cryptography;

public readonly record struct Identifier(string Id)
{
    /// <summary>
    /// Genera un nuevo Identifier único tipo GUID acortado.
    /// </summary>
    public static Identifier New()
    {
        // Genera un GUID
        var guid = Guid.NewGuid();

        // Convierte a Base36 para acortar
        var id = ToBase36(guid);

        return new Identifier(id);
    }

    public static Identifier Parse(string value) => new Identifier(value);

    public override string ToString() => Id;

    // ---------------- Helpers ----------------

    private static string ToBase36(Guid guid)
    {
        var bytes = guid.ToByteArray();
        var number = new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray()); // BigInteger requiere byte extra
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var result = "";
        while (number > 0)
        {
            var rem = (int)(number % 36);
            result = chars[rem] + result;
            number /= 36;
        }
        return result;
    }
}
