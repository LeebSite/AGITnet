namespace AGITnet.Domain.Services;

public static class ProductionBalancer
{
    public static int[] Balance(int[] quantities)
    {
        if (quantities == null || quantities.Length == 0)
            return Array.Empty<int>();

        ValidateInput(quantities);

        var activeSlots = GetActiveSlots(quantities);

        if (activeSlots.Count == 0)
            return new int[quantities.Length];

        int total = 0;
        foreach (var slot in activeSlots)
            total += slot.OriginalQuantity;

        int baseQuantity = total / activeSlots.Count;
        int remainder = total % activeSlots.Count;

        // Sisa pembagian didistribusikan ke slot dengan original quantity terbesar.
        // Jika original quantity sama, slot dengan index lebih kecil mendapat prioritas.
        activeSlots.Sort((a, b) =>
        {
            int cmp = b.OriginalQuantity.CompareTo(a.OriginalQuantity);
            return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
        });

        var result = new int[quantities.Length];

        for (int i = 0; i < activeSlots.Count; i++)
        {
            int extra = i < remainder ? 1 : 0;
            result[activeSlots[i].Index] = baseQuantity + extra;
        }

        return result;
    }

    private static void ValidateInput(int[] quantities)
    {
        for (int i = 0; i < quantities.Length; i++)
        {
            if (quantities[i] < 0)
                throw new ArgumentException($"Quantity pada slot {i} tidak boleh negatif.");
        }
    }

    private static List<(int Index, int OriginalQuantity)> GetActiveSlots(int[] quantities)
    {
        var activeSlots = new List<(int Index, int OriginalQuantity)>();

        for (int i = 0; i < quantities.Length; i++)
        {
            // Slot dengan quantity 0 dianggap tidak aktif dan tidak ikut pembagian.
            if (quantities[i] > 0)
                activeSlots.Add((i, quantities[i]));
        }

        return activeSlots;
    }
}
