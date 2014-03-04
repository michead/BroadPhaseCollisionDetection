__kernel void ccArrayCreation(          __global const ulong* in,
                                        __global ulong* out,
                                        __global uint* temp,
                                        __global uint* numOfOcc,
                                        __global uint* temp2,
                                        __global uint* ccIndexes,
                                        __global uint* flags){
                                    
    int i = get_global_id(0);          
    
    if(temp2[i] == 1){
        out[temp[i] - 1] = in[i];
        if(flags[i] == 1){
            ccIndexes[temp[i] - 1] = numOfOcc[(uint)in[i]];
        }
    }
}