public enum FontStyle{
    Gr, Go, LR, SR
}

// enums, in order that the sprites appear
public enum HUDFont{
    Gr0, Gr1, Gr2, Gr3, Gr4, Gr5, Gr6, Gr7, Gr8, Gr9,
    Go0, Go1, Go2, Go3, Go4, Go5, Go6, Go7, Go8, Go9,
    LR0, LR1, LR2, LR3, LR4, LR5, LR6, LR7, LR8, LR9, LRPcnt, LRNeg,
    SR0, SR1, SR2, SR3, SR4, SR5, SR6, SR7, SR8, SR9, 
    SRA, SRB, SRC, SRD, SRE, SRF, SRG, SRH, SRI, SRJ, SRK, SRL, SRM, SRN, SRO, SRP, SRQ, SRR, SRS, SRT, SRU, SRV, SRW, SRX, SRY, SRZ,
    SRQuo, SRCol, SRSCol, SRPipe, SRParL, SRParR, SREqu, SRPlus, SRCarL, SRCarR, SRDblq, SRPnd, SRDol, SRPcnt, SRNeg, SRAt, SRSqbL, SRSqbR, SRAst,
    SRPow, SRAmp, SRUndsc, SRQst, SRExc,
    LRA, LRB, LRC, LRD, LRE, LRF, LRG, LRH, LRI, LRJ, LRK, LRL, LRM, LRN, LRO, LRP, LRQ, LRR, LRS, LRT, LRU, LRV, LRW, LRX, LRY, LRZ,
    LRa, LRb, LRc, LRd, LRe, LRf, LRg, LRh, LRi, LRj, LRk, LRl, LRm, LRn, LRo, LRp, LRq, LRr, LRs, LRt, LRu, LRv, LRw, LRx, LRy, LRz,
    LRQst, LRExc, LRParL, LRUndsc, LRunused1, LRPlus, LRParR, LREqu, LRunused2, LRFsl, LRBsl, LRDblq, LRQuo, LRCol, LRSCol, LRCarL, LRCarR, LRPnd, LRDol, LRAst, LRPow,
    
    SRFsl, SRBsl, SRPrd,
    WhiteSp
}

public enum AmmoType{
    None, Bullet, Shell, Rocket, Cell, Enemy
}

public enum ArmorType{
    None, Blue = 2, Green = 3 
}

public enum PickupType{
    Health, Armor, Weapon, Ammo, Key, Backpack
}

public enum EnemyState{
    Dead, Sleep,
    Injured,
    RangedAttack, MeleeAttack, Chase
}
