﻿using UnityEngine;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine.EventSystems;
using System;
using UnityEngine.AI;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField] float playerSpeed = 1f;
        [SerializeField] GameObject movementIndicator;

        Health health;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float weaponCursorRange = 5f;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;


        private void Awake()
        {
            health = GetComponent<Health>();
        }

        void Update()
        {
            if (InteractWithUI()) { return; }
            if (health.IsDead())
            {
                SetCursor(CursorType.Dead);
                return;
            }

            if (InteractWithComponent()) { return; }

            if (InteractWithMovement()) { return; }
            SetCursor(CursorType.None);
        }

        public float GetWeaponCursorRange()
        {
            return weaponCursorRange;
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        private bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }

        public bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if (!GetComponent<Mover>().CanMoveTo(target)) { return false; }
                if (Input.GetMouseButton(0))
                {
                    GetComponent<Mover>().StartMoveAction(target, playerSpeed);
                    // SetMoveIndicator(target);
                }
                else if (Input.GetButton("Vertical"))
                {
                    Vector3 destination;
                    destination = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z * Input.GetAxis("Vertical"));
                    print(Input.GetAxis("Vertical"));
                    target = transform.position + destination;
                    GetComponent<Mover>().StartMoveAction(target, playerSpeed);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        private RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }

        public void SetMoveIndicator(Vector3 target)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Instantiate(movementIndicator, (target + (Vector3.up / 8)), Quaternion.Euler(-90, 0, 0));
            }
        }

        public bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit) { return false; }

            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);

            if (!hasCastToNavMesh) { return false; }

            target = navMeshHit.position;

            return true;
        }



        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
