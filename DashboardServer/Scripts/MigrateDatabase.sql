-- マイグレーションスクリプト: 診療科マスタと病棟マスタの拡張
-- 実行日: 2025-11-21

-- ==================================================
-- 1. 診療科マスタの拡張
-- ==================================================

-- 既存の診療科テーブルをバックアップ
CREATE TABLE IF NOT EXISTS 診療科_backup AS SELECT * FROM 診療科;

-- 診療科テーブルを削除して再作成
DROP TABLE IF EXISTS 診療科_new;
CREATE TABLE 診療科_new (
    診療科ID TEXT PRIMARY KEY,
    診療科名 TEXT NOT NULL,
    SEQ INTEGER NOT NULL DEFAULT 0,
    isDisplay INTEGER NOT NULL DEFAULT 1,  -- SQLiteではBooleanはINTEGER (0=false, 1=true)
    Color TEXT
);

-- 既存データを移行（SEQは元の順番、isDisplayはすべてtrue、Colorはデフォルト色）
INSERT INTO 診療科_new (診療科ID, 診療科名, SEQ, isDisplay, Color)
SELECT 
    診療科ID, 
    診療科名,
    CASE 診療科名
        WHEN '内科' THEN 1
        WHEN '小児科' THEN 2
        WHEN '整形外科' THEN 3
        ELSE 99
    END as SEQ,
    1 as isDisplay,
    CASE 診療科名
        WHEN '内科' THEN '#ef4444'      -- red-500
        WHEN '小児科' THEN '#3b82f6'    -- blue-500
        WHEN '整形外科' THEN '#f59e0b'  -- amber-500
        ELSE '#8b5cf6'                  -- purple-500
    END as Color
FROM 診療科;

-- 古いテーブルを削除して新しいテーブルを置き換え
DROP TABLE 診療科;
ALTER TABLE 診療科_new RENAME TO 診療科;

-- ==================================================
-- 2. 病棟マスタの作成
-- ==================================================

CREATE TABLE IF NOT EXISTS 病棟 (
    病棟ID TEXT PRIMARY KEY,
    病棟名 TEXT NOT NULL,
    SEQ INTEGER NOT NULL DEFAULT 0,
    isDisplay INTEGER NOT NULL DEFAULT 1,
    Color TEXT
);

-- 既存の入院患者テーブルから病棟名を抽出して病棟マスタに投入
INSERT OR IGNORE INTO 病棟 (病棟ID, 病棟名, SEQ, isDisplay, Color)
SELECT DISTINCT 
    病棟 as 病棟ID,
    病棟 as 病棟名,
    ROW_NUMBER() OVER (ORDER BY 病棟) as SEQ,
    1 as isDisplay,
    '#10b981' as Color  -- emerald-500
FROM 入院患者
WHERE 病棟 IS NOT NULL AND 病棟 != '';

-- ==================================================
-- 3. インデックスの作成
-- ==================================================

CREATE INDEX IF NOT EXISTS idx_診療科_SEQ ON 診療科(SEQ);
CREATE INDEX IF NOT EXISTS idx_診療科_isDisplay ON 診療科(isDisplay);
CREATE INDEX IF NOT EXISTS idx_病棟_SEQ ON 病棟(SEQ);
CREATE INDEX IF NOT EXISTS idx_病棟_isDisplay ON 病棟(isDisplay);

-- ==================================================
-- 4. 確認用クエリ
-- ==================================================

-- 診療科マスタの確認
-- SELECT * FROM 診療科 ORDER BY SEQ;

-- 病棟マスタの確認
-- SELECT * FROM 病棟 ORDER BY SEQ;
