# RandomLevel 文档
---
## 主流程

0. GenerateMesh  &emsp; 在一定范围内，生成指定个数大小范围的RoomMesh（由凸包或者圆组成，可重叠）
1. CollsionSimulate &emsp; 根据碰撞规则重新模拟碰撞计算出RoomMesh的最终位置
2. FilterMinor &emsp; 剔除不符合规则的RoomMesh
3. GenerateEdge &emsp; 构建房间之间的连通图，根据连通图生成EdgeMesh
4. GenerateDoor &emsp; 根据EdgeMesh与RoomMesh的碰撞关系生成DoorMesh
5. GenerateVoxel &emsp; 分别将每一层的LevelMesh进行体素化，并标记区域类型
6. GenerateGameplayLevel &emsp; 根据Voxel重新构建连通关系，并标记Wall区域，Monster区域等
7. InitLevelStartEnd &emsp;根据新的连通关系，找出符合要求的起点房间，终点房间，并计算所以房间的对应深度
8. BuildGameplay &emsp; 生成房间关卡配置（涉及TaskEditor模块）
9. BuildScene &emsp; 构建场景

## 主要目录

```
RandomLevel
|
└───Debugger    Mono代码，入口
|   |   LevelDebugger   RandomLevel的总入口，参数设置
|   |   LevelMeshDebugger   与LevelMesh对应的Mono类，主要用于Unity的显示
|   |   LevelEdgeDebugger   与LevelEdge对应的Mono类，主要用于Unity的显示
|   |   LevelPanelDebugger  与LevelPanel对应的Mono类，主要用于Unity的显示
|   |   ...
|   
└───Editor      Debugger的Editor代码
|   |   ...
|   
└───Helper    
|   |   Delaunay    三角形剖分
|   |   GeometryHelper    计算几何帮助类
|   |   SpanningTree    最小生成树
|   
└───SceneMap    构建场景相关 
|   |
│   │   LevelScene 场景构建方法类
│   │   LevelGraph SceneLevel管理类(LevelMesh,LevelCell)
|   |   LevelMesh Mesh基类
|   |   LevelPanel 继承于LevelMesh,RoomMesh的基类
|   |   LevelEdge 继承于LevelMesh,EdgeMesh的对象类
|   |   LevelCell 体素化的对象类
|   |   SceneCell 受LevelCell管理，用于记录Scene相关的Mask数据
│   │
│   └───Panel   RoomMesh类拓展
│       │   CirclePanel 圆形
│       │   RectPanel 矩形
│       │   ...
│   
└───Gameplay    构建自动生成Gameplay关卡需要的数据
    |
    │   GameplayLevel   GameplayLevel管理类(LevelArea,LevelGroup)
    |   LevelArea   GameplayLevel区域基类
    │   LevelRoom   继承于LevelArea,房间对象类
    |   LevelCorridor 继承于LevelArea,走廊对象类
    |   LevelGroup  继承于LevelArea,多Cell组成的Group基类
    |
    └───Group   Group类拓展
        |   
        |   Door    门Group
        |   MonsterGroup    怪Group
        |   ...
```

## 主要参数
### **SceneLevel**
####  GenerateMesh 
* Width  &emsp;随机撒点的大小范围
* Height  &emsp;随机撒点的大小范围
* InitCount  &emsp;初始撒点数
#### FilterMinor
* NeedMainPanel  &emsp;符合大小的房间数
* RoomFilter  &emsp;房间的面积阈值
* PointRandomRandge  &emsp;房间的随机宽高范围
#### CollsionSimulate
* RoomSkinWidth  &emsp;房间Mesh的用于碰撞的额外外拓
#### GenerateEdge
* MixPersentes   &emsp;三角形剖分采用的百分比
* EdgeWidth  &emsp;走廊的宽度
#### GenerateVoxel
* CellSize   &emsp;生成的Cell的大小（可以有小数位，小数位需为2的幂，即0.5,0.25,0.125）
### **GameplayLevel**
* MinDepth   &emsp;构成起点终点的最小深度
### **Other**
* MeshMaterial   &emsp;LevelMesh的表现材质
* VoxelMeshMaterial  &emsp;由Voxel组成的Graph的Mesh的表现材质


