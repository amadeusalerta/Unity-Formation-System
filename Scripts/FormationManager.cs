using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Drawing;
using System;

[Serializable]
public class FleetFormSlot
{
    public Vector3 Position;
    public Quaternion Rotation;
    public GameObject Occupant;
    public int slotID;

    public FleetFormSlot(Vector3 pos, Quaternion rot, int slot)
    {
        Position = pos;
        Rotation = rot;
        slotID = slot;
    }
}

public class FormationManager : MonoBehaviour
{
    [ReadOnly] public List<FleetFormSlot> fleetFormSlots = new();
    [SerializeField] private Color debugColor;

    [Title("Formation Configurations")]
    [SerializeField] private int width;
    [SerializeField] private int depth;
    [SerializeField] private float spacing;

    [Title("Formation Offset")]
    [SerializeField] private float _xOffset;
    [SerializeField] private float _zOffset;


    public Action OnKilled;

    private void OnEnable()
    {
        OnKilled += RecalcultatePosition;
    }
    private void OnDisable()
    {
        OnKilled -= RecalcultatePosition;
    }

    private void Awake()
    {
        if (fleetFormSlots.Count == 0) CreateFormationGrid();
    }

    private void CreateFormationGrid()
    {
        fleetFormSlots.Clear();

        int count = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float xPos = (x - (width - 1) / 2f) * spacing;
                float zPos = (z + (depth - 1) / 2f) * spacing;

                Vector3 localPos = new Vector3(xPos + _xOffset, 0, zPos + _zOffset);
                Quaternion localRot = Quaternion.identity;

                fleetFormSlots.Add(new FleetFormSlot(localPos, localRot, count));
                count++;
            }
        }
    }

    public void Update()
    {
        using (Draw.InLocalSpace(transform))
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xPos = (x - (width - 1) / 2f) * spacing;
                    float zPos = (z + (depth - 1) / 2f) * spacing;

                    Vector3 localPos = new Vector3(xPos + _xOffset, 0, zPos + _zOffset);

                    Draw.WireBox(localPos, Vector3.one * 0.5f, debugColor);
                    Draw.Label2D(localPos, $"Slot [{x},{z}]", 12f);
                    Draw.Arrow(localPos, localPos + Vector3.forward * 0.7f, debugColor);
                }
            }
        }
    }

    public FleetFormSlot GetSlot(GameObject unit)
    {
        if (fleetFormSlots.Count == 0) CreateFormationGrid();

        foreach (FleetFormSlot slot in fleetFormSlots)
        {
            if (slot.Occupant == null)
            {
                slot.Occupant = unit;
                return slot;
            }
        }
        return null;
    }

    public FleetFormSlot SetSlot(GameObject unit)
    {
        if (fleetFormSlots.Count == 0) CreateFormationGrid();

        foreach (FleetFormSlot slot in fleetFormSlots)
        {
            if (slot.Occupant == unit) return slot;
        }
        return null;
    }
    public void NotifyUnitKilled(GameObject unit)
    {
        for (int i = 0; i < fleetFormSlots.Count; i++)
        {
            if (fleetFormSlots[i].Occupant == unit)
            {
                fleetFormSlots[i].Occupant = null;
                break;
            }
        }

        RecalcultatePosition();
    }

    public void RecalcultatePosition()
    {
        for (int i = 0; i < fleetFormSlots.Count - 1; i++)
        {
            int nextSlot = i + 1;
            if (fleetFormSlots[i].Occupant == null && fleetFormSlots[nextSlot].Occupant != null)
            {
                fleetFormSlots[i].Occupant = fleetFormSlots[nextSlot].Occupant;
                fleetFormSlots[nextSlot].Occupant = null;
            }
        }
    }
}
