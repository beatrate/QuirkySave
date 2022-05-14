using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuirkySave.Examples
{
    public class ExampleSavedComponent : MonoBehaviour, ISavedComponent
    {
        [SaveField]
        private bool someBool;
        [SaveField]
        private Vector3 someVector;

        public bool ShouldSave()
        {
            return true;
        }

		public void Save()
        {

        }

		public void Load()
        {

        }
        
        public void Start()
        {
            // Thanks to execution order the loading has already run by this point at Start().
        }
    }
}
