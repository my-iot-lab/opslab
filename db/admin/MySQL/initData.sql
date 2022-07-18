-- ADMIN/123789
-- User/123789
DELETE From Users where UserName in ('Admin', 'User');
INSERT INTO Users (UserName, Password, PassSalt, DisplayName, RegisterTime, ApprovedTime, ApprovedBy, Description) values ('Admin', 'Es7WVgNsJuELwWK8daCqufUBknCsSC0IYDphQZAiGOo=', 'W5vpBEOYRGHkQXatN0t+ECM/U8cHDuEgrq56+zZBk4J481xH', 'Administrator', now(), now(), 'system', '系统默认创建');
INSERT INTO Users (UserName, Password, PassSalt, DisplayName, RegisterTime, ApprovedTime, ApprovedBy, Description, App) values ('User', 'tXG/yNffpnm6cThrCH7wf6jN1ic3VHvLoY4OrzKtrZ4=', 'c5cIrRMn8XjB84M/D/X7Lg9uUqQFmYNEdxb/4HWH8OLa4pNZ', '测试账号', now(), now(), 'system', '系统默认创建', 'Demo');

DELETE From Dicts Where Define = 0;
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('菜单', '系统菜单', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('菜单', '外部菜单', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('应用程序', '后台管理', 'BA', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '网站标题', '管理系统', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '网站页脚', '2022 © 通用后台管理系统', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统通知', '用户注册', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统通知', '程序异常', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统通知', '数据库连接', '2', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('通知状态', '未处理', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('通知状态', '已处理', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('处理结果', '同意', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('处理结果', '拒绝', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('消息状态', '未读', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('消息状态', '已读', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('消息标签', '一般', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('消息标签', '紧要', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('头像地址', '头像路径', '/images/uploader/', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('头像地址', '头像文件', 'default.jpg', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站样式', '蓝色样式', 'blue.css', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站样式', '黑色样式', 'black.css', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站样式', 'AdminLTE', 'lte.css', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '使用样式', 'blue.css', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '前台首页', '~/Home/Index', 0);

-- 网站UI设置
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '侧边栏状态', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '卡片标题状态', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '固定表头', '1', 0);

-- 登录配置
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '短信验证码登录', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', 'OAuth 认证登录', '0', 0);

-- 自动锁屏（秒）默认 30 秒
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '自动锁屏时长', '30', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '自动锁屏', '0', 0);

-- 是否启用 Blazor 默认为 0 未启用
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', 'Blazor', '1', 0);

-- 是否启用 健康检查 默认为 0 未启用 1 启用
INSERT INTO Dicts (Category, Name, Code, Define) Values ('网站设置', '健康检查', '0', 0);

-- 时长单位 月
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '程序异常保留时长', '1', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '操作日志保留时长', '12', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '登录日志保留时长', '12', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '访问日志保留时长', '1', 0);

-- 时长单位 天
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', 'Cookie保留时长', '7', 0);

INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', 'IP地理位置接口', 'None', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置服务', '百度地图开放平台', 'BaiDuIPSvr', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置服务', '聚合地理位置', 'JuheIPSvr', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置服务', '百度138地理位置', 'BaiDuIP138Svr', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置', 'BaiDuIPSvr', 'http://api.map.baidu.com/location/ip?ak=6lvVPMDlm2gjLpU0aiqPsHXi2OiwGQRj&ip=', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置', 'JuheIPSvr', 'http://apis.juhe.cn/ip/ipNew?key=f57102d1b9fadd3f4a1c29072d0c0206&ip=', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('地理位置', 'BaiDuIP138Svr', 'https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?resource_id=6006&query=', 0);

-- 时长单位 分钟
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', 'IP请求缓存时长', '10', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '演示系统', '0', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '授权盐值', 'yjglE2eddCGcS7tTFTDd2DfvqXHgCnMhNhpmx9HJaC9l8GAZ', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '哈希结果', '6jTT50HGuk8V+AIsiE4IfqjcER71PBN1DY7gqOLZE7E=', 0);

INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '验证码图床', 'http://imgs.sdgxgz.com/images/', 0);

INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '默认应用程序', '0', 0);

INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '后台地址', 'http://localhost:5000', 0);

-- 系统登录首页设置
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统首页', '高仿码云', 'Login-Gitee', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统首页', '蓝色清新', 'Login-Blue', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统首页', '系统默认', 'Login', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统首页', '科技动感', 'Login-Tec', 0);
INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('系统首页', 'Admin-LTE', 'Login-LTE', 0);

INSERT INTO Dicts (Category, Name, Code, Define) VALUES ('网站设置', '登录界面', 'Login', 0);

-- 菜单
DELETE FROM Navigations Where Category = '0';
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (0, '系统管理', 10, 'fa fa-gear', '#', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '个人中心', 10, 'fa fa-suitcase', '~/Admin/Profiles', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '保存显示名称', 10, 'fa fa-fa', 'saveDisplayName', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '保存密码', 20, 'fa fa-fa', 'savePassword', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '保存应用', 30, 'fa fa-fa', 'saveApp', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '保存样式', 40, 'fa fa-fa', 'saveTheme', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 4, '保存头像', 50, 'fa fa-fa', 'saveIcon', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 5, '保存网站设置', 60, 'fa fa-fa', 'saveUISettings', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '网站设置', 20, 'fa fa-fa', '~/Admin/Settings', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '保存系统名称', 10, 'fa fa-fa', 'saveTitle', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '保存页脚设置', 20, 'fa fa-fa', 'saveFooter', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '保存样式', 30, 'fa fa-fa', 'saveTheme', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '清理缓存', 40, 'fa fa-fa', 'clearCache', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 4, '清理全部缓存', 50, 'fa fa-fa', 'clearAllCache', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 5, '登录设置', 60, 'fa fa-fa', 'loginSettings', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 6, '自动锁屏', 70, 'fa fa-fa', 'lockScreen', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 7, '默认应用', 80, 'fa fa-fa', 'defaultApp', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '菜单管理', 30, 'fa fa-dashboard', '~/Admin/Menus', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '新增', 10, 'fa fa-fa', 'add', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '编辑', 20, 'fa fa-fa', 'edit', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '删除', 30, 'fa fa-fa', 'del', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '分配角色', 40, 'fa fa-fa', 'assignRole', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (1, '图标页面', 40, 'fa fa-fa', '~/Admin/IconView', '0', 1);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (1, '侧边栏', 50, 'fa fa-fa', '~/Admin/Sidebar', '0', 1);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '用户管理', 60, 'fa fa-user', '~/Admin/Users', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '新增', 10, 'fa fa-fa', 'add', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '编辑', 20, 'fa fa-fa', 'edit', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '删除', 30, 'fa fa-fa', 'del', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '分配部门', 40, 'fa fa-fa', 'assignGroup', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 4, '分配角色', 50, 'fa fa-fa', 'assignRole', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '角色管理', 70, 'fa fa-sitemap', '~/Admin/Roles', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '新增', 10, 'fa fa-fa', 'add', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '编辑', 20, 'fa fa-fa', 'edit', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '删除', 30, 'fa fa-fa', 'del', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '分配用户', 40, 'fa fa-fa', 'assignUser', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 4, '分配部门', 50, 'fa fa-fa', 'assignGroup', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 5, '分配菜单', 60, 'fa fa-fa', 'assignMenu', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 6, '分配应用', 70, 'fa fa-fa', 'assignApp', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '部门管理', 80, 'fa fa-bank', '~/Admin/Groups', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '新增', 10, 'fa fa-fa', 'add', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '编辑', 20, 'fa fa-fa', 'edit', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '删除', 30, 'fa fa-fa', 'del', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 3, '分配用户', 40, 'fa fa-fa', 'assignUser', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 4, '分配角色', 50, 'fa fa-fa', 'assignRole', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '字典表维护', 90, 'fa fa-book', '~/Admin/Dicts', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '新增', 10, 'fa fa-fa', 'add', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '编辑', 20, 'fa fa-fa', 'edit', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 2, '删除', 30, 'fa fa-fa', 'del', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '站内消息', 100, 'fa fa-envelope', '~/Admin/Messages', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '任务管理', 110, 'fa fa fa-tasks', '~/Admin/Tasks', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '暂停', 10, 'fa fa-fa', 'pause', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity - 1, '日志', 20, 'fa fa-fa', 'info', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '通知管理', 120, 'fa fa-bell', '~/Admin/Notifications', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '系统日志', 130, 'fa fa-gears', '#', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity, '操作日志', 10, 'fa fa-edit', '~/Admin/Logs', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity - 1, '登录日志', 20, 'fa fa-user-circle-o', '~/Admin/Logins', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity - 2, '访问日志', 30, 'fa fa-bars', '~/Admin/Traces', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity - 3, 'SQL日志', 40, 'fa fa-database', '~/Admin/SQL', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '在线用户', 140, 'fa fa-users', '~/Admin/Online', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '网站分析', 150, 'fa fa-line-chart', '~/Admin/Analyse', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '程序异常', 160, 'fa fa-cubes', '~/Admin/Exceptions', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category, IsResource) VALUES (@@identity, '服务器日志', 10, 'fa fa-fa', 'log', '0', 2);
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '健康检查', 170, 'fa fa-heartbeat', '~/Admin/Healths', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (1, '工具集合', 180, 'fa fa-gavel', '#', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity, '客户端测试', 10, 'fa fa-wrench', '~/Admin/Mobile', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity - 1, 'API文档', 10, 'fa fa-wrench', '~/swagger', '0');
INSERT INTO Navigations (ParentId, Name, `Order`, Icon, Url, Category) VALUES (@@identity - 2, '图标集', 10, 'fa fa-dashboard', '~/Admin/FAIcon', '0');

-- 角色和用户
DELETE FROM `Groups` WHERE GroupName = 'Admin';
INSERT INTO `Groups` (GroupCode, GroupName, Description) VALUES ('001', 'Admin', '系统默认组');

DELETE FROM Roles where RoleName in ('Administrators', 'Default');
INSERT INTO Roles (RoleName, Description) VALUES ('Administrators', '系统管理员');
INSERT INTO Roles (RoleName, Description) VALUES ('Default', '默认用户');

-- Admin 拥有所有权限
DELETE FROM RoleGroup;
INSERT INTO RoleGroup (GroupId, RoleId) SELECT g.Id, r.Id From `Groups` g left join Roles r on 1=1 where GroupName = 'Admin' and RoleName = 'Administrators';

DELETE FROM UserGroup;

DELETE FROM UserRole;
INSERT INTO UserRole (UserId, RoleId) 
SELECT u.Id, r.Id From Users u left join Roles r on 1=1 where UserName = 'Admin' and RoleName = 'Administrators';

INSERT INTO UserRole (UserId, RoleId) 
SELECT u.Id, r.Id From Users u left join Roles r on 1=1 where UserName = 'User' and RoleName = 'Default';

DELETE FROM NavigationRole;
INSERT INTO NavigationRole (NavigationID, RoleID) SELECT n.Id, r.Id FROM Navigations n left join Roles r on 1=1 Where RoleName = 'Administrators';

INSERT INTO NavigationRole (NavigationID, RoleID) 
SELECT n.Id, r.Id FROM Navigations n left join Roles r on 1=1 Where RoleName = 'Default' and Name in ('个人中心', '通知管理');

INSERT INTO NavigationRole (NavigationID, RoleID) 
SELECT n.Id, r.Id FROM Navigations n left join Roles r on 1=1 Where RoleName = 'Default' and ParentId in (select id from Navigations where Name in ('个人中心'));


-- 角色对应用授权
DELETE From RoleApp where AppId in ('BA');
INSERT INTO RoleApp (AppId, RoleId) SELECT 'BA', ID From Roles Where RoleName IN ('Administrators', 'Default');
