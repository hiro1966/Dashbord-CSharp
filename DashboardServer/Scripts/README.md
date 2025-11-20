# ダミーデータ作成スクリプト

## 📋 概要

病院ダッシュボード用のダミーデータを生成するスクリプトです。

## 🗄️ データベース設計

### テーブル構成

#### 1. 診療科テーブル

| カラム名 | データ型 | 制約 | 説明 |
|---------|---------|------|------|
| 診療科ID | TEXT | PRIMARY KEY | 2文字の診療科コード |
| 診療科名 | TEXT | NOT NULL | 診療科名（20文字以内） |

**初期データ:**
- `01` - 内科
- `02` - 小児科
- `03` - 整形外科

#### 2. 入院患者テーブル

| カラム名 | データ型 | 制約 | 説明 |
|---------|---------|------|------|
| 年月日 | TEXT | PRIMARY KEY (複合) | YYYY-MM-DD形式 |
| 診療科ID | TEXT | PRIMARY KEY (複合), FK | 診療科コード |
| 病棟 | TEXT | PRIMARY KEY (複合) | 病棟名（例: 3階病棟） |
| 入院患者数 | INTEGER | NOT NULL | 入院患者数 |
| 退院患者数 | INTEGER | NOT NULL | 退院患者数 |
| 転入患者数 | INTEGER | NOT NULL | 転入患者数 |
| 転出患者数 | INTEGER | NOT NULL | 転出患者数 |

**病棟:**
- 3階病棟
- 4階病棟
- 5階病棟
- 6階病棟

#### 3. 外来患者テーブル

| カラム名 | データ型 | 制約 | 説明 |
|---------|---------|------|------|
| 年月日 | TEXT | PRIMARY KEY (複合) | YYYY-MM-DD形式 |
| 診療科ID | TEXT | PRIMARY KEY (複合), FK | 診療科コード |
| 初再診 | INTEGER | PRIMARY KEY (複合) | 0:初診, 1:再診 |
| 患者数 | INTEGER | NOT NULL | 患者数 |

## 🚀 使い方

### 方法1: バッチファイル実行（Windows）

```cmd
cd DashboardServer\Scripts
create-dummy-data.bat
```

### 方法2: 直接実行

```cmd
cd DashboardServer\Scripts
dotnet run --project CreateDummyData.csproj -- ..\dashboard.db
```

## 📊 生成されるデータ

### 期間
- **開始日**: 2025-01-01
- **終了日**: 2025-10-31
- **日数**: 304日

### データ量

#### 診療科
- **件数**: 3件

#### 入院患者
- **日数**: 304日
- **診療科**: 3科
- **病棟**: 4病棟
- **合計**: 304 × 3 × 4 = **3,648件**

各日・診療科・病棟の組み合わせでランダム生成:
- 入院患者数: 5～24人
- 退院患者数: 0～7人
- 転入患者数: 0～4人
- 転出患者数: 0～4人

#### 外来患者
- **日数**: 304日
- **診療科**: 3科
- **初再診**: 2パターン（初診・再診）
- **合計**: 304 × 3 × 2 = **1,824件**

各日・診療科の組み合わせでランダム生成:
- 初診患者数: 10～39人
- 再診患者数: 30～79人

### 合計データ数
- **全体**: 3 + 3,648 + 1,824 = **5,475件**

## 🔧 カスタマイズ

### 期間変更

`CreateDummyData.cs` の以下の部分を変更:

```csharp
var startDate = new DateTime(2025, 1, 1);
var endDate = new DateTime(2025, 10, 31);
```

### 診療科追加

1. `InsertDepartmentsAsync()` で診療科を追加
2. `Departments` 配列に診療科IDを追加

### 病棟変更

`Wards` 配列を変更:

```csharp
private static readonly string[] Wards = { "3階病棟", "4階病棟", "5階病棟", "6階病棟" };
```

### 患者数範囲変更

`random.Next(最小値, 最大値)` の値を変更

## 📝 SQL例

### 診療科別入院患者数（最新日付）

```sql
SELECT 
    s.診療科名,
    SUM(i.入院患者数) as 入院患者数,
    SUM(i.退院患者数) as 退院患者数
FROM 入院患者 i
INNER JOIN 診療科 s ON i.診療科ID = s.診療科ID
WHERE i.年月日 = (SELECT MAX(年月日) FROM 入院患者)
GROUP BY s.診療科名;
```

### 診療科別外来患者数（月別）

```sql
SELECT 
    strftime('%Y-%m', o.年月日) as 年月,
    s.診療科名,
    SUM(CASE WHEN o.初再診 = 0 THEN o.患者数 ELSE 0 END) as 初診,
    SUM(CASE WHEN o.初再診 = 1 THEN o.患者数 ELSE 0 END) as 再診,
    SUM(o.患者数) as 合計
FROM 外来患者 o
INNER JOIN 診療科 s ON o.診療科ID = s.診療科ID
GROUP BY strftime('%Y-%m', o.年月日), s.診療科名
ORDER BY 年月, s.診療科名;
```

### 病棟別入院患者推移

```sql
SELECT 
    i.年月日,
    i.病棟,
    SUM(i.入院患者数) as 入院患者数
FROM 入院患者 i
GROUP BY i.年月日, i.病棟
ORDER BY i.年月日, i.病棟;
```

## ⚠️ 注意事項

- **既存データ削除**: 実行すると入院患者・外来患者テーブルのデータが削除されます
- **診療科保持**: 診療科テーブルは既存データがある場合は保持されます
- **ランダムシード固定**: 毎回同じデータが生成されます（開発用）

## 🔄 データ再生成

データを再生成したい場合:

1. `create-dummy-data.bat` を再実行
2. 既存の入院患者・外来患者データが削除され、新しいデータが生成されます

## 📁 ファイル構成

```
Scripts/
├── CreateDummyData.cs          # メインスクリプト
├── CreateDummyData.csproj      # プロジェクトファイル
├── create-dummy-data.bat       # 実行バッチ（Shift-JIS）
└── README.md                   # このファイル
```
