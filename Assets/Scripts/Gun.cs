/**
  * Gun class to store the gun's information that is retrievable at any time.
  */
// public class Gun{

//         public Ammo ammo { get; private set; }
//         public AmmoType ammoType;

//         public float fireRate{ get; }
//         public float projectileDist{ get; }
//         public float projectileSpeed{ get; }
//         public float bulletSpread{ get; }
        
//         public string gunName{ get; }

//         public Gun(string _gunName, float _fireRate, float _projectileDist, float _projectileSpeed, AmmoType _ammoType, float _bulletSpread){
//             gunName = _gunName;
//             fireRate = _fireRate;
//             projectileDist = _projectileDist;
//             projectileSpeed = _projectileSpeed;
//             ammoType = _ammoType;
//             bulletSpread = _bulletSpread;
//         }
        
//         /*
//          * Setting the ammo, because every ammo type is share between guns, the gun doesn't store the ammo, only a reference of
//          */
//         public void SetAmmo(ref Ammo _ammo){
//             ammo = _ammo;
//         }
        
//         /*
//          * The gun expends the ammo, but doesn't keep track of the ammo directly. Returns true on successful expendature.
//          */
//         public bool ExpendAmmo(){
//             if(ammo == null)
//                 return false;

//             return ammo.ExpendAmmo();
//         }

//         public override string ToString(){
//             return string.Format("Name: {0} - Ammo Type: {1} - Current Ammo: {2} - Max Ammo: {3} - Fire Rate: {4} - Projectile Speed: {5} - Projectile Distance: {6}", gunName, ammo.type, ammo.count, ammo.max, fireRate, projectileSpeed, projectileDist);
//         }
// }

/**
  * Ammo class to store the ammo information, since ammo is shared between guns.
  */
public class Ammo{
    public int max{ get; private set; }
    public int multiplier{ get; private set; }
    public int count{ get; private set;}
    public AmmoType type{ get; }

    public Ammo(AmmoType _type){
        type = _type;
        count = 0;
        multiplier = 1;
        SetMaxAmmo();
    }

    /*
     * Adds ammo to the current pool. Returns true if it successfully added ammo.
     */
    public bool AddAmmo(int ammoToAdd){
        // checking if it's type none first, which cannot be added ammo, and shouldn't ever be picked up, but just in case.
        // we should never be picking up max that is -1, as it should be infinite 
        // but to prevent issues, we'll check for it anyway.
        if(type == AmmoType.None || (count >= max && max != -1 && count != -1))
            return false;
        
        if(count + ammoToAdd > max)
            count = max;
        else
            count += ammoToAdd;
            
        return true;
    }

    /*
     * Expends ammo, returns true if it successfully expended ammo. 
     */
    public bool ExpendAmmo(){
        // -1 means it will expend ammo successfully no matter what, and we still return as true without subtracting ammo.
        if(count == -1)
            return true;

        if(count == 0)
            return false;

        count--;
        return true;
    }

    public void IncreaseMaxAmmo(int _multiplier){
        if(_multiplier > 0){
            multiplier = _multiplier;
            SetMaxAmmo();
        }
    }

    /*
     * Sets the max ammo based on the type of ammo, defaults to -1 (infinite) if the ammo is undefined.
     */
    void SetMaxAmmo(){
        switch(type){
            case AmmoType.None: count = -1; max = -1; break;
            case AmmoType.Bullet: max = 200 * multiplier; break;
            case AmmoType.Shell: max = 50 * multiplier; break;
            case AmmoType.Rocket: max = 50 * multiplier; break;
            case AmmoType.Cell: max = 300 * multiplier; break;
        }
    }

    public override string ToString(){
        return string.Format("Type: {0} Count: {1}, Max Ammo: {2} ", type, count, max);
    }
}
