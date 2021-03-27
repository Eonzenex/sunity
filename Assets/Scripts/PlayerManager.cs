using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sunity.Game
{
    public class PlayerManager : MonoBehaviour
    {
        #region Singleton

        private static PlayerManager _singleton;

        public static PlayerManager Singleton { get { return _singleton; } }

        private void Awake()
        {
            if (_singleton != null && _singleton != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _singleton = this;
            }
        }

        #endregion

        #region UI

        public GameObject PlayerUI;

        #endregion

        #region All Player Info

        public List<NetworkObject> PlayerList { get; set; }

        #endregion

        #region Local Player Details

        public GameObject LocalPlayer { get; set; }

        #endregion

        #region Unity Lifecycle Methods

        // Start is called before the first frame update
        void Start()
        {
            PlayerList = new List<NetworkObject>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion
    }
}