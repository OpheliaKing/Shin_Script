using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class ResourceManager : ManagerBase
    {
        private readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>();

        [Header("리소스 경로")]
        [Tooltip("Prefab이 있는 폴더 (Resources 폴더 기준 하위 경로)")]
        [SerializeField]
        private string _prefabFolderPath = "Prefab";

        [Tooltip("사운드가 있는 폴더 (Resources 폴더 기준 하위 경로)")]
        [SerializeField]
        private string _soundFolderPath = "Sound";

        [Tooltip("아트(스프라이트/텍스처 등)가 있는 폴더 (Resources 폴더 기준 하위 경로)")]
        [SerializeField]
        private string _artFolderPath = "Art";

        /// <summary>
        /// 하위 폴더 + 리소스명으로 Resources 폴더 기준 전체 경로를 만듭니다.
        /// </summary>
        /// <param name="subFolder">Resources 기준 하위 폴더명 (예: Prefab, Sound)</param>
        /// <param name="resourceName">리소스 파일명 (확장자 제외)</param>
        public string GetResourcePath(string subFolder, string resourceName)
        {
            bool hasFolder = !string.IsNullOrEmpty(subFolder);
            bool hasName = !string.IsNullOrEmpty(resourceName);

            if (!hasFolder)
                return hasName ? resourceName : "";
            if (!hasName)
                return subFolder;
            return subFolder + "/" + resourceName;
        }

        /// <summary> Prefab 폴더 기준 경로 (예: Prefab/Player) </summary>
        public string GetPrefabPath(string resourceName) => GetResourcePath(_prefabFolderPath, resourceName);

        /// <summary> Sound 폴더 기준 경로 (예: Sound/BGM) </summary>
        public string GetSoundPath(string resourceName) => GetResourcePath(_soundFolderPath, resourceName);

        /// <summary> Art 폴더 기준 경로 (예: Art/Icon) </summary>
        public string GetArtPath(string resourceName) => GetResourcePath(_artFolderPath, resourceName);

        /// <summary>
        /// Resources 폴더 기준 경로에서 리소스를 로드합니다. 캐시에 있으면 캐시된 것을 반환합니다.
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로 (확장자 제외, 예: "Prefabs/Player")</param>
        public T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (_cache.TryGetValue(path, out Object cached))
                return cached as T;

            T asset = Resources.Load<T>(path);
            if (asset != null)
                _cache[path] = asset;

            return asset;
        }

        /// <summary>
        /// 지정한 하위 폴더에서 리소스를 로드합니다. (캐시 사용)
        /// </summary>
        /// <param name="subFolder">Resources 기준 하위 폴더 (예: Prefab, Sound, Art)</param>
        /// <param name="resourceName">리소스명 (확장자 제외)</param>
        public T LoadInFolder<T>(string subFolder, string resourceName) where T : Object =>
            Load<T>(GetResourcePath(subFolder, resourceName));

        /// <summary> Prefab 폴더에서 로드 (캐시 사용) </summary>
        public T LoadPrefab<T>(string resourceName) where T : Object => LoadInFolder<T>(_prefabFolderPath, resourceName);

        /// <summary> Prefab 폴더에서 로드 후 인스턴스 생성 </summary>
        public T LoadPrefabAndInstantiate<T>(string resourceName, Transform parent = null) where T : Object =>
            LoadAndInstantiate<T>(GetPrefabPath(resourceName), parent);

        /// <summary> Sound 폴더에서 로드 (캐시 사용, 예: AudioClip) </summary>
        public T LoadSound<T>(string resourceName) where T : Object => LoadInFolder<T>(_soundFolderPath, resourceName);

        /// <summary> Art 폴더에서 로드 (캐시 사용, 예: Sprite, Texture2D) </summary>
        public T LoadArt<T>(string resourceName) where T : Object => LoadInFolder<T>(_artFolderPath, resourceName);

        /// <summary>
        /// 리소스를 로드한 뒤 인스턴스를 생성합니다. 로드 시 캐시를 사용합니다.
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로 (확장자 제외)</param>
        /// <param name="parent">부모 Transform (null 가능)</param>
        public T LoadAndInstantiate<T>(string path, Transform parent = null) where T : Object
        {
            T prefab = Load<T>(path);
            if (prefab == null)
                return null;

            return parent != null ? Object.Instantiate(prefab, parent) : Object.Instantiate(prefab);
        }

        /// <summary>
        /// 특정 경로의 캐시를 제거합니다.
        /// </summary>
        public void UnloadCache(string path)
        {
            _cache.Remove(path);
        }

        /// <summary>
        /// 모든 캐시를 비웁니다.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
