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
