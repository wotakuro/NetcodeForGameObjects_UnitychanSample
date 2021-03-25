using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using System.CodeDom;
using System.Reflection;
using MLAPI;

namespace UTJ.MLAPISample
{
    // Network関係のUtility関数
    public class NetworkUtility
    {
        private static bool isHeadlessResult;
        private static bool isHeadlessCache = false;

        // BatchMode 起動かどうかを調べて返します
        public static bool IsBatchModeRun
        {
            get
            {
#if ENABLE_AUTO_CLIENT
                if (isHeadlessCache)
                {
                    return isHeadlessResult;
                }
                isHeadlessResult = IsBatchMode();
                isHeadlessCache = true;
#else
                isHeadlessResult = false;
#endif
                return isHeadlessResult;
            }
        }
        // localのIPアドレスを調べて返します
        public static string GetLocalIP()
        {
            string ipaddress = "";
            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipaddress = ip.ToString();
                    break;
                }
            }
            return ipaddress;
        }


        // Batch Mode起動かどうか調べて返します
        private static bool IsBatchMode()
        {
            var commands = System.Environment.GetCommandLineArgs();

            foreach( var command in commands)
            {
                if(command.ToLower().Trim() == "-batchmode")
                {
                    return true;
                }
            }
            return (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }


        // PlayerLoopを利用してHeadless起動時に余計なものを削除します
        public static void RemoveUpdateSystemForHeadlessServer()
        {
            RemoveUpdateSystem(ShouldExcludeForHeadless, true);
        }
        // PlayerLoopを利用してBatchMode起動時に余計なものを削除します
        public static void RemoveUpdateSystemForBatchBuild()
        {
            RemoveUpdateSystem(ShouldExcludeForHeadless, false);
        }

        // 余計なプレイヤーループを消す関数
        private static void RemoveUpdateSystem(System.Func< PlayerLoopSystem,bool> shouldExcludeFunc,bool removeAllPhysics)
        { 
            var currentLoop = PlayerLoop.GetCurrentPlayerLoop();
            var replaceSubSystems = new List<PlayerLoopSystem>();
            var replaceUpdateSystems = new List<PlayerLoopSystem>();

            foreach ( var subsystem in currentLoop.subSystemList)
            {
                // 物理丸っと消したい人向け
                if (removeAllPhysics &&
                    subsystem.type == typeof(UnityEngine.PlayerLoop.FixedUpdate))
                {
                    continue;
                }
                
                replaceUpdateSystems.Clear();
                var newSubSystem = subsystem;

                foreach ( var updateSystem in subsystem.subSystemList)
                {
                    if (!shouldExcludeFunc(updateSystem))
                    {
                        replaceUpdateSystems.Add(updateSystem);
                    }
                }
                newSubSystem.subSystemList = replaceUpdateSystems.ToArray();
                replaceSubSystems.Add(newSubSystem);
            }
            currentLoop.subSystemList = replaceSubSystems.ToArray();

            PlayerLoop.SetPlayerLoop(currentLoop);
        }

        // Headlessの状況でサブシステムを削る所
        private static bool ShouldExcludeForHeadless(PlayerLoopSystem updateSystem)
        {
            return
                // マウスイベントを削除
                (updateSystem.type == typeof(PreUpdate.SendMouseEvents)) ||
                // Inputいらない
                (updateSystem.type == typeof(PreUpdate.NewInputUpdate)) ||
                // Audioの更新もいらない
                (updateSystem.type == typeof(PostLateUpdate.UpdateAudio)) ||
                // Animationの類を消す
                (updateSystem.type == typeof(PreLateUpdate.DirectorUpdateAnimationBegin)) ||
                (updateSystem.type == typeof(PreLateUpdate.DirectorDeferredEvaluate)) ||
                (updateSystem.type == typeof(PreLateUpdate.DirectorUpdateAnimationEnd)) ||
                (updateSystem.type == typeof(Update.DirectorUpdate)) ||
                (updateSystem.type == typeof(PreLateUpdate.LegacyAnimationUpdate)) ||
                (updateSystem.type == typeof(PreLateUpdate.ConstraintManagerUpdate)) ||
                // Particleの類を消す
                (updateSystem.type == typeof(PreLateUpdate.ParticleSystemBeginUpdateAll)) ||
                (updateSystem.type == typeof(PostLateUpdate.ParticleSystemEndUpdateAll)) ||
                // Videoの類を消す
                (updateSystem.type == typeof(PostLateUpdate.UpdateVideoTextures)) ||
                (updateSystem.type == typeof(PostLateUpdate.UpdateVideo)) ||
                // Rendererの更新消す
                (updateSystem.type == typeof(PostLateUpdate.UpdateAllRenderers)) ||
                (updateSystem.type == typeof(PostLateUpdate.UpdateAllSkinnedMeshes)) ||
                // Canvasもいらない
                (updateSystem.type == typeof(PostLateUpdate.PlayerUpdateCanvases)) ||
                (updateSystem.type == typeof(PostLateUpdate.PlayerEmitCanvasGeometry)) ||
                // AI Updateもいらない
                (updateSystem.type == typeof(PreUpdate.AIUpdate)) ||
                (updateSystem.type == typeof(PreLateUpdate.AIUpdatePostScript)) ||
                false;
        }


        // Headlessで必要であることを表します
        public class RequireAtHeadless:System.Attribute
        {
        }

        // Standaloneでしかいらないようなコンポーネントを削除します
        public static void RemoveAllStandaloneComponents(GameObject gmo)
        {
            var allComponents = gmo.GetComponentsInChildren<MonoBehaviour>(true);
            foreach( var component in allComponents)
            {
                if(component is MLAPI.NetworkObject || 
                    component is MLAPI.NetworkBehaviour)
                {
                    continue;
                }
                var attr = component.GetType().GetCustomAttribute(typeof(RequireAtHeadless), false);
                if( attr != null){
                    continue;
                }

                Object.Destroy(component);
            }
        }

    }
}