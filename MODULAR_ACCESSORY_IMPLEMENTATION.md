# Modular Weapon Accessory Implementation

## Overview
This implementation adds functionality to allow weapon accessories to be detached from parent weapons and attached to others, including the ability to mount accessories onto other accessories (e.g., slide mount allowing mounting another mount onto it).

## Key Features

### 1. Modular Accessory System
- **PlugsIntoModularMount**: Defines what mount type an accessory can plug into
- **HasModularMount**: Defines what mount type an accessory provides for other accessories
- **IsModularCurrentlyEquipped**: Tracks whether an accessory is currently equipped and contributing to weapon stats
- **ChangeModularEquip**: Method to equip/unequip modular accessories

### 2. Enhanced Mount System
- Updated `GetAccessoryMounts()` and `GetAccessoryMountsAsync()` to include mounts from accessories that have modular mounts
- Enhanced `CheckAccessoryRequirements()` to validate modular mount compatibility
- Support for accessory-to-accessory mounting

### 3. User Interface
- Added "Detach Accessory" and "Attach Accessory" context menu options
- Created `SelectWeaponAccessoryTarget` form for selecting attachment targets
- Added appropriate language strings for all new UI elements

## Implementation Details

### Backend Changes

#### WeaponAccessory.cs
- Added new fields: `_strHasModularMount`, `_strPlugsIntoModularMount`
- Added properties: `PlugsIntoModularMount`, `HasModularMount`
- Added async properties: `GetPlugsIntoModularMountAsync()`, `GetHasModularMountAsync()`
- Added modular equip tracking: `IsModularCurrentlyEquipped`, `GetIsModularCurrentlyEquippedAsync()`
- Added modular equip methods: `ChangeModularEquip()`, `ChangeModularEquipAsync()`
- Updated XML loading/saving to handle new fields

#### Weapon.cs
- Enhanced `GetAccessoryMounts()` to include mounts from accessories with modular mounts
- Enhanced `CheckAccessoryRequirements()` to validate modular mount compatibility
- Updated both sync and async versions of these methods

### Frontend Changes

#### CharacterCareer.cs
- Added `tsWeaponAccessoryDetach_Click()` event handler for detaching accessories
- Added `tsWeaponAccessoryAttach_Click()` event handler for attaching accessories
- Both handlers include proper validation and user confirmation

#### CharacterCareer.Designer.cs
- Added new context menu items: `tsWeaponAccessoryDetach`, `tsWeaponAccessoryAttach`
- Updated context menu to include new options

#### SelectWeaponAccessoryTarget.cs
- New form for selecting target weapons/accessories for attachment
- Validates compatibility between source and target mounts
- Shows only compatible targets

### Language Support
- Added menu strings: `Menu_DetachAccessory`, `Menu_AttachAccessory`
- Added message strings for validation and confirmation dialogs
- Added form labels and titles for the new selection dialog

## XML Data Structure

### New Fields
- `<mountsto>`: Defines what mount type an accessory can plug into
- `<modularmount>`: Defines what mount type an accessory provides

### Example Usage
```xml
<!-- Slide Mount that provides a "Slide" mount for other accessories -->
<accessory>
  <id>slide-mount</id>
  <name>Slide Mount</name>
  <mount>Top/Under/Side</mount>
  <modularmount>Slide</modularmount>
  <avail>4</avail>
  <cost>500</cost>
</accessory>

<!-- Laser Sight that can plug into a Slide Mount -->
<accessory>
  <id>laser-sight-slide</id>
  <name>Laser Sight (Slide Mount)</name>
  <mount>Slide</mount>
  <mountsto>Slide</mountsto>
  <avail>4</avail>
  <cost>200</cost>
  <accuracy>1</accuracy>
</accessory>
```

## Usage Examples

### Detaching an Accessory
1. Right-click on a modular accessory in the weapons tree
2. Select "Detach Accessory" from the context menu
3. Confirm the detachment
4. The accessory is removed from its current weapon and becomes available for reattachment

### Attaching an Accessory
1. Right-click on a detached modular accessory
2. Select "Attach Accessory" from the context menu
3. Select a compatible weapon or accessory from the target selection dialog
4. The accessory is attached to the selected target

### Accessory-to-Accessory Mounting
1. Install a Slide Mount on a weapon
2. The Slide Mount provides a "Slide" mount type
3. Install accessories that can plug into "Slide" mounts (e.g., Laser Sight, Tactical Light)
4. These accessories can be detached and reattached to other Slide Mounts

## Benefits

1. **Flexibility**: Accessories can be moved between weapons as needed
2. **Modularity**: Accessories can be mounted on other accessories, creating complex mounting systems
3. **Realism**: Reflects real-world weapon customization where accessories can be swapped
4. **User Experience**: Intuitive right-click context menu for easy accessory management
5. **Validation**: Proper mount compatibility checking prevents invalid configurations

## Future Enhancements

1. **Storage System**: Implement a proper storage system for detached accessories
2. **Bulk Operations**: Allow multiple accessories to be detached/attached at once
3. **Mount Visualization**: Visual representation of mount compatibility in the UI
4. **Advanced Mounting**: Support for more complex mounting hierarchies
5. **Performance Optimization**: Optimize mount validation for large numbers of accessories

## Testing

The implementation includes:
- Proper error handling and validation
- User confirmation dialogs
- Thread-safe operations using async/await patterns
- Integration with existing weapon accessory systems
- Backward compatibility with existing accessories

## Files Modified

### Backend
- `Chummer/Backend/Equipment/WeaponAccessory.cs`
- `Chummer/Backend/Equipment/Weapon.cs`

### Frontend
- `Chummer/Forms/Character Forms/CharacterCareer.cs`
- `Chummer/Forms/Character Forms/CharacterCareer.Designer.cs`
- `Chummer/Forms/Selection Forms/SelectWeaponAccessoryTarget.cs` (new)
- `Chummer/Forms/Selection Forms/SelectWeaponAccessoryTarget.Designer.cs` (new)

### Language
- `Chummer/lang/en-us.xml`

### Documentation
- `example_modular_accessory.xml` (example data)
- `MODULAR_ACCESSORY_IMPLEMENTATION.md` (this file)
