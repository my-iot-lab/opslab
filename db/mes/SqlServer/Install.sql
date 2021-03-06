
-- 基础管理
-- 物料管理（LES）
-- 设备管理（MRO）

-- 生产管理（PES）
-- 质量管理（QMS）

--------------------------------
-- 基础资料
--------------------------------

-- 工序
-- 工艺路线
-- 工艺参数

-- 工站信息
CREATE TABLE [dbo].[Station](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Code]				[varchar](32) NOT NULL, -- 工站编码
	[Name]				[nvarchar](64) NOT NULL, -- 工站描述
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)

-- 工序
CREATE TABLE [dbo].[Process](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Code]				[varchar](32) NOT NULL, -- 工站编码
	[Name]				[nvarchar](64) NOT NULL, -- 工站描述
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)

-- 工艺路线
CREATE TABLE [dbo].[ProcessFlow](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Next]				[varchar](32) NULL, -- 下一工站
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)

-- 物料基础信息
CREATE TABLE [dbo].[Material](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Code]				[varchar](32) NOT NULL, -- 物料代码
	[Name]				[nvarchar](64) NOT NULL, -- 物料名称
	[Model]				[varchar](64) NULL,	-- 规格型号
	[Attr]				[varchar](16) NULL,	-- 成品、半成品、原材料
	[Unit]				[varchar](16) NOT NULL, -- 计量单位，如 PCS、件、kg 等
	[]					[varchar](16) NULL, -- 自制、外购、委外加工、虚拟件
	[BarcodeRule]		[varchar](256) NOT NULL, -- 条码规则，多个以逗号分隔
	[Expiration]		[int] NULL,	-- 保质期
	[Version] 			[int] NOT NULL, -- 版本
	[CreatedBy]			[varchar](32) NULL,
	[CreatedAt]			[datetime] NOT NULL,
	[UpdatedBy]			[varchar](32) NULL,
	[UpdatedAt]			[datetime] NULL,
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)

-- BOM
CREATE TABLE [dbo].[Bom](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Code]				[varchar](32) NOT NULL, -- BOM 编号
	[Index]				[int] NOT NULL, -- 加工序号
	[UnitQty]			[int] NOT NULL, -- 单位用量
	[Version] 			[int] NOT NULL, -- 版本
	[CreatedBy]			[varchar](32) NULL,
	[CreatedAt]			[datetime] NOT NULL,
	[UpdatedBy]			[varchar](32) NULL,
	[UpdatedAt]			[datetime] NULL,
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)	

-- 消耗的物料
CREATE TABLE [dbo].[Material](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Station]			[varchar](64) NOT NULL, -- 工站编码
	[ProductCode] 		[varchar](64) NOT NULL, -- 产品码
	[SN] 				[varchar](64) NOT NULL, -- SN（注：同一个工站 SN 可存档多次）
	[Barcode]			[varchar](64) NOT NULL, -- 物料码
	[]					[int] NOT NULL, -- 物料分类（关键物料/批次料）
	Qty					[int] NOT NULL,	-- 用料数量
	[CreatedBy]			[varchar](32) NULL,
	[CreatedAt]			[datetime] NOT NULL,
	CONSTRAINT PK_Material PRIMARY KEY CLUSTERED ([Id])
)	

CREATE INDEX Index_Material_Barcode ON Material (Barcode);
CREATE INDEX Index_Material_SN ON Material (SN);

-- 进站数据
CREATE TABLE [dbo].[Archive](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Station]			[varchar](64) NOT NULL, -- 工站编码
	[ProductCode] 		[varchar](64) NOT NULL, -- 产品码
	[OrderNo] 			[varchar](64) NULL, -- 工单号（冗余），可为 NULL
	[SN] 				[varchar](64) NOT NULL, -- SN（注：同一个工站 SN 可存档多次）
	[Tray]				[varchar](32) NULL, -- 托盘号
	[CreatedBy]			[varchar](32) NULL,
	[CreatedAt]			[datetime] NOT NULL,
	[UpdatedBy]			[varchar](32) NULL,
	[UpdatedAt]			[datetime] NULL,
	CONSTRAINT PK_Archive PRIMARY KEY CLUSTERED ([Id])
)

CREATE INDEX Index_Archive_SN ON ArchiveData (SN);

-- 产品存档主数据（出站）
CREATE TABLE [dbo].[Archive](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[Station]			[varchar](64) NOT NULL, -- 工站编码
	[ProductCode] 		[varchar](64) NOT NULL, -- 产品码
	[OrderNo] 			[varchar](64) NULL, -- 工单号（冗余），可为 NULL
	[SN] 				[varchar](64) NOT NULL, -- SN（注：同一个工站 SN 可存档多次）
	[Pass] 				[int] NOT NULL, -- 过站结果 10->OK;15->强制OK;20->NG;25->强制NG
	[Operator]			[varchar](32) NULL, -- 操作人
	[Tray]				[varchar](32) NULL, -- 托盘号
	[CreatedBy]			[varchar](32) NULL,
	[CreatedAt]			[datetime] NOT NULL,
	[UpdatedBy]			[varchar](32) NULL,
	[UpdatedAt]			[datetime] NULL,
	CONSTRAINT PK_Archive PRIMARY KEY CLUSTERED ([Id])
)

CREATE INDEX Index_Archive_SN ON ArchiveData (SN);

-- 产品存档详细数据（出站）
CREATE TABLE [dbo].[ArchiveItem](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[ArchiveId]			[int] NOT NULL, -- 主数据 ID
	[Tag]				[varchar](32) NULL, -- 触发点
	[Name] 				[nvarchar](64) NOT NULL, -- 名称
	[Value] 			[varchar](1024) NULL, -- 值，数组以逗号隔开
	[IsArray]			[bit] NOT NULL DEFAULT 0, -- 是否是数组值
	[Higher]			[DECIMAL(12, 2)] NULL, -- 上限值
	[Lower]				[DECIMAL(12, 2)] NULL, -- 下限值
	CONSTRAINT PK_ArchiveItem PRIMARY KEY CLUSTERED ([Id])
)

CREATE INDEX Index_ArchiveItem_ArchiveId ON ArchiveItem (ArchiveId);

-- 产品存档详细数据，将数组数据拆开（出站）
CREATE TABLE [dbo].[ArchiveItemLine](
	[Id] 				[int] IDENTITY(1,1) NOT NULL,
	[ArchiveItemId]		[int] NOT NULL, -- 主数据 ID
	[Tag]				[varchar](32) NULL, -- 触发点
	[Item] 				[nvarchar](64) NOT NULL, -- 名称，按序号展开
	[Value] 			[DECIMAL(12, 2) NOT NULL, -- 值
	[Higher]			[DECIMAL(12, 2)] NULL, -- 上限值
	[Lower]				[DECIMAL(12, 2)] NULL, -- 下限值
	CONSTRAINT PK_ArchiveItem PRIMARY KEY CLUSTERED ([Id])
)

CREATE INDEX Index_ArchiveItemLine_ArchiveItemId ON ArchiveItemLine (ArchiveItemId);

-- 返工
