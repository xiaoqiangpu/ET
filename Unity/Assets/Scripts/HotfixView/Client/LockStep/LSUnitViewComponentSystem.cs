using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 帧同步-实体(Unit)实体渲染层组件系统
    /// </summary>
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    public static partial class LSUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this LSUnitViewComponent self)
        {

        }

        public static async ETTask InitAsync(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            Scene root = self.Root();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
                GameObject bundleGameObject = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
                GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

                GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();
                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.AddComponent<LSAnimatorComponent>();
                Log.Info($"pxq---帧同步--加载实体单元--例如玩家:{unitGo.name}");
            }
        }
    }
}