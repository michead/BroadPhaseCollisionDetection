__kernel void elementCount(  __global const ulong* in,
                                    __global uint* temp,
                                    __global uint* n,
                                    __global uint* occPerRad,
                                    __global uint* flags){
    
    int i = get_global_id(0);
   
    if((uint)in[i] != 4294967295){
        atom_inc(&occPerRad[(uint)in[i]]);
    }
    
    barrier(CLK_GLOBAL_MEM_FENCE);
    
    if((i==0 || ((uint)in[i] != (uint)in[i-1])) && (in[i] & ((ulong)1 << 63)) && ((uint)in[i] != 4294967295)){
        temp[i] = 1;
        flags[i] = 1;
        atom_inc(n);
        
        
        for(uint p = 1; p < occPerRad[(uint)in[i]]; p++){
            temp[i + p] = 1;
            atom_inc(n);
        }
    }
}