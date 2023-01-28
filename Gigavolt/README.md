# GigaVoltage
## 简介 Introduction
这是一个为生存战争游戏带来十亿伏特电力系统的mod，将原版的16个电压级别（0\~1.5V）扩展到2^32个（0\~2^32-1V）

This is a mod for Survivalcraft that take a new Electric system with Gigavolt to the game. The original Electric system has 16 voltage level(0\~1.5V), then Gigavolt expands it to 2^32 voltage level(0\~2^32-1V).
## 区别 Differences
|方块|原版|十亿伏特版|
|--|--|--|
|SR锁存器|S端高压才储存|S端非0即储存|
|开关、按钮、电池|默认输出1.5V|默认输出0xFFFFFFFF V|
|计数器|默认溢出电压为0x10V，上限为0xF V|默认溢出电压为0V，上限0xFFFFFFFF V，可设置溢出电压|
|存储器|略|需先手动设置存储矩阵长宽才可使用，长宽上限为2^31-1；每个数字用英文逗号,分开，每行数字用英文句号.分开|
|压力板|有压力时输出0.8\~1.5V，随意设置的压力与电压关系|输出准确的压力值，参考16进制结果：男性玩家0x46V，虎鲸0x5DC V|
|彩色LED、1面LED|0.8\~1.5V对应不同颜色|所有电压对应不同颜色（ABGR格式）|
|靶子|输出0.8\~1.5V|输出0\~0xFFFFFF00 V，因游戏坐标精度问题，最低2位为0|