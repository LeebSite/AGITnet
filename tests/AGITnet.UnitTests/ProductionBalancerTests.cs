using AGITnet.Domain.Services;

namespace AGITnet.UnitTests;

public class ProductionBalancerTests
{
    [Fact]
    public void Balance_SampleAssessment_MenghasilkanDistribusiYangBenar()
    {
        var input = new[] { 4, 5, 1, 7, 6, 4, 0 };
        var expected = new[] { 4, 5, 4, 5, 5, 4, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_SemuaQuantitySama_TidakBerubah()
    {
        var input = new[] { 3, 3, 3 };
        var expected = new[] { 3, 3, 3 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_PembagianHabis_DistribusiMerata()
    {
        // Total 12 / 3 slot aktif = 4 per slot
        var input = new[] { 2, 6, 4 };
        var expected = new[] { 4, 4, 4 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_PembagianMemilikiSisa_SisaDiberikanKeOriginalTerbesar()
    {
        // Total 10 / 3 slot = 3 sisa 1. Slot index 1 (original 5) mendapat tambahan.
        var input = new[] { 3, 5, 2 };
        var expected = new[] { 3, 4, 3 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_SemuaSlotNol_HasilTetapNol()
    {
        var input = new[] { 0, 0, 0 };
        var expected = new[] { 0, 0, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_SatuSlotAktif_SeluruhTotalDiSlotTersebut()
    {
        var input = new[] { 0, 10, 0 };
        var expected = new[] { 0, 10, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_TieBreaker_IndexLebihKecilMendapatPrioritas()
    {
        // Total 13 / 3 slot = 4 sisa 1. Slot 0 dan 1 sama-sama original 5.
        // Slot 0 (index lebih kecil) mendapat prioritas.
        var input = new[] { 5, 5, 3 };
        var expected = new[] { 5, 4, 4 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_QuantityNegatif_ThrowArgumentException()
    {
        var input = new[] { 4, -1, 3 };

        Assert.Throws<ArgumentException>(() => ProductionBalancer.Balance(input));
    }

    [Fact]
    public void Balance_BanyakSlot_DistribusiTetapBenar()
    {
        // 10 slot aktif + 2 slot tidak aktif. Total 50 / 10 = 5 per slot, habis.
        var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 5, 0, 0 };
        var expected = new[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 0, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_OriginalSangatTidakMerata_TetapDibalance()
    {
        // Total 102 / 3 slot = 34 per slot, habis.
        var input = new[] { 100, 1, 1 };
        var expected = new[] { 34, 34, 34 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_TotalQuantityInvariant_JumlahTetapSama()
    {
        var input = new[] { 4, 5, 1, 7, 6, 4, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(input.Sum(), result.Sum());
    }

    [Fact]
    public void Balance_SlotTidakAktifTetapNol_SetelahBalancing()
    {
        var input = new[] { 0, 8, 0, 4, 0 };
        var expected = new[] { 0, 6, 0, 6, 0 };

        var result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Balance_ArrayKosong_KembalikanArrayKosong()
    {
        var input = Array.Empty<int>();

        var result = ProductionBalancer.Balance(input);

        Assert.Empty(result);
    }

    [Fact]
    public void Balance_PerbedaanSlotAktifMaksimalSatu()
    {
        var input = new[] { 10, 1, 1, 0 };

        var result = ProductionBalancer.Balance(input);

        // Ambil hanya slot aktif
        var activeValues = new List<int>();
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] > 0)
                activeValues.Add(result[i]);
        }

        int max = activeValues.Max();
        int min = activeValues.Min();
        Assert.True(max - min <= 1, $"Perbedaan slot aktif ({max} - {min} = {max - min}) melebihi 1.");
    }
}
