using System;
using UnityEngine;

public readonly struct LocationInfo : IEquatable<LocationInfo>
{
    public static readonly LocationInfo None = new LocationInfo(false, default, null);

    public bool HasArea { get; }
    public BuildingFloor Floor { get; }
    public string RoomName { get; }

    private LocationInfo(bool hasArea, BuildingFloor floor, string roomName)
    {
        HasArea = hasArea;
        Floor = floor;
        RoomName = roomName;
    }

    public static LocationInfo At(Vector2 point) => From(LocationArea.At(point));

    public static LocationInfo At(Vector2 point, BuildingFloor likelyFloor) => From(LocationArea.At(point, likelyFloor));

    private static LocationInfo From(LocationArea area)
    {
        return area != null ? new LocationInfo(true, area.Floor, area.DisplayName) : None;
    }

    public bool Equals(LocationInfo other)
    {
        return HasArea == other.HasArea && Floor == other.Floor && RoomName == other.RoomName;
    }

    public override bool Equals(object obj) => obj is LocationInfo other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(HasArea, Floor, RoomName);

    public static bool operator ==(LocationInfo left, LocationInfo right) => left.Equals(right);

    public static bool operator !=(LocationInfo left, LocationInfo right) => !left.Equals(right);
}
