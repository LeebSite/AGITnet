-- =============================================================================
-- AGITnet - Software Engineer Take-Home Assessment
-- Candidate Token: VEH-GHALIBCANDIDATE
-- =============================================================================

-- =============================================================================
-- TASK 1: DDL (Data Definition Language)
-- Membuat struktur tabel plannings dan planning_slots dengan constraint lengkap.
-- =============================================================================

-- Hapus tabel lama jika ada (untuk pengujian ulang script DDL)
DROP TABLE IF EXISTS planning_slots CASCADE;
DROP TABLE IF EXISTS plannings CASCADE;

-- Tabel Header: Rencana Produksi
CREATE TABLE plannings (
    planning_id     SERIAL PRIMARY KEY,
    request_code    VARCHAR(100) NOT NULL,
    candidate_token VARCHAR(100) NOT NULL DEFAULT 'VEH-GHALIBCANDIDATE',
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status          VARCHAR(50)  NOT NULL DEFAULT 'Balanced',
    
    -- Constraint Unique RequestCode untuk mencegah duplikasi transaksi
    CONSTRAINT uk_plannings_request_code UNIQUE (request_code),
    -- Constraint status tidak boleh berupa string kosong
    CONSTRAINT chk_plannings_status_not_empty CHECK (LENGTH(TRIM(status)) > 0)
);

-- Tabel Detail: Slot Produksi
CREATE TABLE planning_slots (
    planning_slot_id   SERIAL PRIMARY KEY,
    planning_id        INT          NOT NULL,
    slot_order         INT          NOT NULL,
    slot_name          VARCHAR(100) NOT NULL,
    original_quantity  INT          NOT NULL,
    balanced_quantity  INT          NOT NULL,
    is_active          BOOLEAN      NOT NULL DEFAULT TRUE,

    -- Foreign Key dengan Cascading Delete
    CONSTRAINT fk_planning_slots_planning 
        FOREIGN KEY (planning_id) 
        REFERENCES plannings(planning_id) 
        ON DELETE CASCADE,

    -- Constraint Urutan Slot harus positif (> 0)
    CONSTRAINT chk_slots_order_positive CHECK (slot_order > 0),
    
    -- Constraint Kuantitas tidak boleh bernilai negatif
    CONSTRAINT chk_slots_original_qty_non_negative CHECK (original_quantity >= 0),
    CONSTRAINT chk_slots_balanced_qty_non_negative CHECK (balanced_quantity >= 0),
    
    -- Unique constraint urutan slot dalam satu planning
    CONSTRAINT uk_slots_planning_order UNIQUE (planning_id, slot_order)
);


-- =============================================================================
-- TASK 2: SEED DATA
-- Token: VEH-GHALIBCANDIDATE
-- =============================================================================

DO $$
DECLARE
    v_id INT;
BEGIN
    -- 1. Kasus Normal (Kombinasi standar 7 slot)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-001', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot A', 4, 4, TRUE),
        (v_id, 2, 'Slot B', 5, 5, TRUE),
        (v_id, 3, 'Slot C', 1, 4, TRUE),
        (v_id, 4, 'Slot D', 7, 5, TRUE),
        (v_id, 5, 'Slot E', 6, 5, TRUE),
        (v_id, 6, 'Slot F', 4, 4, TRUE),
        (v_id, 7, 'Slot G', 0, 0, FALSE);
    END IF;

    -- 2. Kasus Habis Dibagi (Kuantitas rata sempurna)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-002', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 5, 5, TRUE),
        (v_id, 2, 'Slot 2', 5, 5, TRUE),
        (v_id, 3, 'Slot 3', 5, 5, TRUE);
    END IF;

    -- 3. Kasus Ada Remainder (Sisa Pembagian Diberikan Berdasarkan Original Qty Terbesar)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-003', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 10, 7, TRUE),
        (v_id, 2, 'Slot 2', 5,  6, TRUE),
        (v_id, 3, 'Slot 3', 4,  6, TRUE);
    END IF;

    -- 4. Kasus Semua Zero (Seluruh slot 0 / tidak aktif)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-004', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 0, 0, FALSE),
        (v_id, 2, 'Slot 2', 0, 0, FALSE),
        (v_id, 3, 'Slot 3', 0, 0, FALSE);
    END IF;

    -- 5. Kasus Satu Active Slot (Hanya 1 slot aktif, sisa 0)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-005', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 0,  0,  FALSE),
        (v_id, 2, 'Slot 2', 15, 15, TRUE),
        (v_id, 3, 'Slot 3', 0,  0,  FALSE);
    END IF;

    -- 6. Kasus Tie Breaker (Original Quantity sama, Prioritas SlotOrder lebih kecil)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-006', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        -- Total = 13 / 2 active slots = base 6, remainder 1 -> SlotOrder 1 dapat extra 1
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 6, 7, TRUE),
        (v_id, 2, 'Slot 2', 6, 6, TRUE),
        (v_id, 3, 'Slot 3', 0, 0, FALSE);
    END IF;

    -- 7. Kasus Quantity Besar (Nilai skala puluhan ribu)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-007', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot High 1', 10000, 20000, TRUE),
        (v_id, 2, 'Slot High 2', 20000, 20000, TRUE),
        (v_id, 3, 'Slot High 3', 30000, 20000, TRUE);
    END IF;

    -- 8. Kasus Multiple Ties (Banyak slot memiliki kuantitas sama)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-008', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        -- Total = 31 / 4 active = base 7, remainder 3 -> Slot 1, 2, 3 masing-masing dpt 8
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot A', 10, 8, TRUE),
        (v_id, 2, 'Slot B', 10, 8, TRUE),
        (v_id, 3, 'Slot C', 10, 8, TRUE),
        (v_id, 4, 'Slot D', 1,  7, TRUE);
    END IF;

    -- 9. Kasus Perbedaan Quantity Besar (Kesenjangan ekstrem antar slot)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-009', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        -- Total = 103 / 3 active = base 34, remainder 1 -> Slot 1 dpt 35
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Main Line', 100, 35, TRUE),
        (v_id, 2, 'Sub Line A', 1,  34, TRUE),
        (v_id, 3, 'Sub Line B', 2,  34, TRUE);
    END IF;

    -- 10. Kasus Multi-Slot (8 Slot dengan gabungan aktif dan non-aktif)
    INSERT INTO plannings (request_code, candidate_token, status)
    VALUES ('REQ-SEED-010', 'VEH-GHALIBCANDIDATE', 'Balanced')
    ON CONFLICT (request_code) DO NOTHING
    RETURNING planning_id INTO v_id;

    IF v_id IS NOT NULL THEN
        -- Total = 160 / 6 active = base 26, remainder 4 -> Slot 3 (30), 4 (40), 5 (50), 2 (20) dpt 27
        INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active) VALUES
        (v_id, 1, 'Slot 1', 10, 26, TRUE),
        (v_id, 2, 'Slot 2', 20, 27, TRUE),
        (v_id, 3, 'Slot 3', 30, 27, TRUE),
        (v_id, 4, 'Slot 4', 40, 27, TRUE),
        (v_id, 5, 'Slot 5', 50, 27, TRUE),
        (v_id, 6, 'Slot 6', 0,  0,  FALSE),
        (v_id, 7, 'Slot 7', 0,  0,  FALSE),
        (v_id, 8, 'Slot 8', 10, 26, TRUE);
    END IF;
END $$;


-- =============================================================================
-- TASK 3: RECONCILIATION VALIDATION QUERY
-- Memvalidasi bahwa SUM(original_quantity) = SUM(balanced_quantity) untuk setiap transaksi.
-- =============================================================================

SELECT 
    p.planning_id,
    p.request_code,
    COALESCE(SUM(ps.original_quantity), 0) AS total_original_quantity,
    COALESCE(SUM(ps.balanced_quantity), 0) AS total_balanced_quantity,
    COALESCE(SUM(ps.balanced_quantity), 0) - COALESCE(SUM(ps.original_quantity), 0) AS difference,
    CASE 
        WHEN COALESCE(SUM(ps.original_quantity), 0) = COALESCE(SUM(ps.balanced_quantity), 0) THEN 'VALID (MATCH)'
        ELSE 'INVALID (MISMATCH)'
    END AS reconciliation_status
FROM plannings p
LEFT JOIN planning_slots ps ON p.planning_id = ps.planning_id
GROUP BY p.planning_id, p.request_code
ORDER BY p.planning_id ASC;


-- =============================================================================
-- TASK 4: HISTORY QUERY
-- Menampilkan ringkasan riwayat transaksi diurutkan dari yang terbaru.
-- =============================================================================

SELECT 
    p.request_code,
    p.created_at,
    COUNT(CASE WHEN ps.is_active = TRUE THEN 1 END) AS active_slot_count,
    COUNT(ps.planning_slot_id) AS total_slot_count,
    COALESCE(SUM(ps.original_quantity), 0) AS original_total,
    COALESCE(SUM(ps.balanced_quantity), 0) AS balanced_total,
    p.status
FROM plannings p
LEFT JOIN planning_slots ps ON p.planning_id = ps.planning_id
GROUP BY p.planning_id, p.request_code, p.created_at, p.status
ORDER BY p.created_at DESC;


-- =============================================================================
-- TASK 5: ANOMALY DETECTION QUERY
-- Mendeteksi potensi anomali data integritas / data korupsi.
-- =============================================================================

-- Query 1: Deteksi Header tanpa Slot (Header yatim piatu)
SELECT 
    'HEADER_WITHOUT_SLOTS' AS anomaly_type,
    p.planning_id,
    p.request_code,
    'Planning tidak memiliki detail slot' AS anomaly_description
FROM plannings p
LEFT JOIN planning_slots ps ON p.planning_id = ps.planning_id
WHERE ps.planning_slot_id IS NULL

UNION ALL

-- Query 2: Deteksi Total Mismatch (Total original != Total balanced)
SELECT 
    'TOTAL_MISMATCH' AS anomaly_type,
    p.planning_id,
    p.request_code,
    FORMAT('Total Original (%s) != Total Balanced (%s)', SUM(ps.original_quantity), SUM(ps.balanced_quantity)) AS anomaly_description
FROM plannings p
JOIN planning_slots ps ON p.planning_id = ps.planning_id
GROUP BY p.planning_id, p.request_code
HAVING SUM(ps.original_quantity) <> SUM(ps.balanced_quantity)

UNION ALL

-- Query 3: Deteksi Slot Tidak Aktif tetapi Memiliki Balanced Quantity > 0
SELECT 
    'INACTIVE_SLOT_WITH_QTY' AS anomaly_type,
    p.planning_id,
    p.request_code,
    FORMAT('Slot %s (Order %s) tidak aktif tetapi memegang balanced qty %s', ps.slot_name, ps.slot_order, ps.balanced_quantity) AS anomaly_description
FROM plannings p
JOIN planning_slots ps ON p.planning_id = ps.planning_id
WHERE ps.is_active = FALSE AND ps.balanced_quantity > 0

UNION ALL

-- Query 4: Deteksi Kuantitas Negatif (Data corruption)
SELECT 
    'NEGATIVE_QUANTITY' AS anomaly_type,
    p.planning_id,
    p.request_code,
    FORMAT('Slot %s memiliki qty negatif (Orig: %s, Bal: %s)', ps.slot_name, ps.original_quantity, ps.balanced_quantity) AS anomaly_description
FROM plannings p
JOIN planning_slots ps ON p.planning_id = ps.planning_id
WHERE ps.original_quantity < 0 OR ps.balanced_quantity < 0;


-- =============================================================================
-- TASK 6: TOP 3 ADJUSTMENT TERBESAR
-- Mengambil 3 slot dengan selisih absolut penyesuaian terbesar.
-- Tie-breaker: slot_order lebih kecil didahulukan.
-- =============================================================================

SELECT 
    p.request_code,
    ps.slot_order,
    ps.slot_name,
    ps.original_quantity,
    ps.balanced_quantity,
    ABS(ps.balanced_quantity - ps.original_quantity) AS adjustment_amount
FROM planning_slots ps
JOIN plannings p ON ps.planning_id = p.planning_id
ORDER BY 
    ABS(ps.balanced_quantity - ps.original_quantity) DESC,
    ps.slot_order ASC
LIMIT 3;


-- =============================================================================
-- TASK 7: ATOMIC TRANSACTION EXAMPLE & ROLLBACK EXPLANATION
-- Contoh eksekusi transaksi atomik (Header + Details) dalam PostgreSQL.
-- =============================================================================

BEGIN;

-- 1. Insert Header Planning
INSERT INTO plannings (request_code, candidate_token, status)
VALUES ('REQ-ATOMIC-001', 'VEH-GHALIBCANDIDATE', 'Balanced');

-- 2. Insert Detail Slot menggunakan ID header yang baru saja dibuat
INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active)
VALUES 
((SELECT planning_id FROM plannings WHERE request_code = 'REQ-ATOMIC-001'), 1, 'Line A', 10, 10, TRUE),
((SELECT planning_id FROM plannings WHERE request_code = 'REQ-ATOMIC-001'), 2, 'Line B', 10, 10, TRUE);

COMMIT;

-- =============================================================================
-- TASK 8: REBALANCE RUN / VERSIONING DESIGN
-- Desain struktur untuk mendukung eksekusi rebalancing ulang (versioning).
-- =============================================================================

-- Hapus tabel jika sudah ada
DROP TABLE IF EXISTS rebalance_runs CASCADE;

-- Tabel Versi Eksekusi Rebalance
CREATE TABLE rebalance_runs (
    run_id         SERIAL PRIMARY KEY,
    planning_id    INT          NOT NULL REFERENCES plannings(planning_id) ON DELETE CASCADE,
    run_number     INT          NOT NULL CHECK (run_number > 0),
    balanced_total INT          NOT NULL CHECK (balanced_total >= 0),
    processed_by   VARCHAR(100) NOT NULL,
    created_at     TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Constraint unik: Satu planning tidak boleh memiliki nomor versi (run_number) yang sama
    CONSTRAINT uk_planning_run_number UNIQUE (planning_id, run_number)
);

-- Contoh Seed Data untuk Versioning
INSERT INTO rebalance_runs (planning_id, run_number, balanced_total, processed_by)
SELECT planning_id, 1, 32, 'VEH-GHALIBCANDIDATE' FROM plannings WHERE request_code = 'REQ-SEED-001'
ON CONFLICT DO NOTHING;

INSERT INTO rebalance_runs (planning_id, run_number, balanced_total, processed_by)
SELECT planning_id, 2, 32, 'VEH-GHALIBCANDIDATE-RECALC' FROM plannings WHERE request_code = 'REQ-SEED-001'
ON CONFLICT DO NOTHING;

-- Query Mengambil Run/Versi Terakhir dari Setiap Planning (Menggunakan DISTINCT ON PostgreSQL)
SELECT DISTINCT ON (r.planning_id)
    p.request_code,
    r.run_id,
    r.run_number AS latest_run_number,
    r.balanced_total,
    r.processed_by,
    r.created_at AS last_run_at
FROM rebalance_runs r
JOIN plannings p ON r.planning_id = p.planning_id
ORDER BY r.planning_id, r.run_number DESC;


-- =============================================================================
-- TASK 9: INDEX PROPOSAL & ANALISIS TRADEOFF
-- Proposal Indeks untuk mengoptimalkan kinerja query produksi.
-- =============================================================================

-- Indeks 1: RequestCode (B-Tree Index)
CREATE INDEX IF NOT EXISTS idx_plannings_request_code ON plannings (request_code);

-- Indeks 2: CreatedAt Descending (Untuk Query Riwayat Terbaru)
CREATE INDEX IF NOT EXISTS idx_plannings_created_at_desc ON plannings (created_at DESC);

-- Indeks 3: Status (Untuk Filtering Status Transaksi)
CREATE INDEX IF NOT EXISTS idx_plannings_status ON plannings (status);

-- Indeks 4: Foreign Key PlanningId pada Detail Slots (Untuk Optimasi JOIN)
CREATE INDEX IF NOT EXISTS idx_planning_slots_planning_id ON planning_slots (planning_id);


-- =============================================================================
-- TASK 10: SAFE MIGRATION STRATEGY (WIDE COLUMNS -> NORMALIZED SLOTS TABLE)
-- Strategi Migrasi Aman dari Tabel Lama Bergaya Wide Columns (slot1_qty, slot2_qty, ...)
-- ke Struktur Normal Terpisah (plannings & planning_slots).
-- =============================================================================

-- Simulasi Tabel Lama (Legacy Structure)
CREATE TABLE IF NOT EXISTS legacy_production_plans (
    legacy_id    SERIAL PRIMARY KEY,
    req_code     VARCHAR(100) UNIQUE NOT NULL,
    slot1_qty    INT DEFAULT 0,
    slot2_qty    INT DEFAULT 0,
    slot3_qty    INT DEFAULT 0,
    created_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Seed Data Contoh pada Tabel Lama
INSERT INTO legacy_production_plans (req_code, slot1_qty, slot2_qty, slot3_qty)
VALUES ('LEGACY-REQ-001', 10, 20, 30)
ON CONFLICT (req_code) DO NOTHING;

-- STEP 1: Validasi Data Lama (Pastikan tidak ada kuantitas negatif di data legacy)
SELECT legacy_id, req_code 
FROM legacy_production_plans 
WHERE slot1_qty < 0 OR slot2_qty < 0 OR slot3_qty < 0;

-- STEP 2 & 3: Migrate & Unpivot Data dari Legacy ke Normalized Structure (LATERAL VALUES)
DO $$
DECLARE
    r RECORD;
    v_new_id INT;
BEGIN
    FOR r IN SELECT * FROM legacy_production_plans LOOP
        -- Insert ke Header Baru
        INSERT INTO plannings (request_code, candidate_token, created_at, status)
        VALUES (r.req_code, 'VEH-GHALIBCANDIDATE-MIGRATED', r.created_date, 'Balanced')
        ON CONFLICT (request_code) DO NOTHING
        RETURNING planning_id INTO v_new_id;

        IF v_new_id IS NOT NULL THEN
            -- Unpivot kolom slot1_qty, slot2_qty, slot3_qty menjadi baris detail
            INSERT INTO planning_slots (planning_id, slot_order, slot_name, original_quantity, balanced_quantity, is_active)
            SELECT 
                v_new_id,
                unpivoted.slot_order,
                unpivoted.slot_name,
                unpivoted.qty,
                unpivoted.qty, -- Asumsi awal balanced qty = original qty
                (unpivoted.qty > 0)
            FROM (
                VALUES 
                    (1, 'Slot 1', r.slot1_qty),
                    (2, 'Slot 2', r.slot2_qty),
                    (3, 'Slot 3', r.slot3_qty)
            ) AS unpivoted(slot_order, slot_name, qty);
        END IF;
    END LOOP;
END $$;

-- STEP 4 & 5: Rekonsiliasi & Validasi Jumlah Row Terpindah
SELECT 
    l.req_code,
    (l.slot1_qty + l.slot2_qty + l.slot3_qty) AS legacy_total,
    SUM(ps.original_quantity) AS new_total,
    CASE 
        WHEN (l.slot1_qty + l.slot2_qty + l.slot3_qty) = SUM(ps.original_quantity) THEN 'MIGRATION SUCCESS'
        ELSE 'MIGRATION MISMATCH'
    END AS status
FROM legacy_production_plans l
JOIN plannings p ON l.req_code = p.request_code
JOIN planning_slots ps ON p.planning_id = ps.planning_id
GROUP BY l.req_code, l.slot1_qty, l.slot2_qty, l.slot3_qty;

-- STEP 6, 7 & 8: Verifikasi Aplikasi & Hapus Tabel Lama secara Aman (Jalankan setelah verifikasi aplikasi)
-- DROP TABLE legacy_production_plans;
