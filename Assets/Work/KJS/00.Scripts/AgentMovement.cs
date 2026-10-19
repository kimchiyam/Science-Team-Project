using System;
using ModuleSystem;
using UnityEngine;

namespace Work.KJS._00.Scripts
{
    public class AgentMovement : MonoModule , IMoveable
    {
        private Rigidbody2D _rigidbody;
        private Vector2 _dir;
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            transform.position.x = 
        }

        public void SetDir(Vector2 dir)
        {
            _dir = dir;
        }


        public void SetDir(System.Numerics.Vector2 dir)
        {
            throw new NotImplementedException();
        }
    }
}