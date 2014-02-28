__kernel void createCollisionCells(     __global const ulong* in, //1024
                                        __global ulong* out, //1024
                                        __global uint* temp, //1024
                                        //__global uint* n, // 1
                                        //__global uint* offset, // 1093
                                        __global uint* numOfOcc,
                                        __global uint* flag){ // 1093
                                    
    int i = get_global_id(0);          
    
    /*if((i==0 || in[i]!=in[i-1]) && (in[i] & ((ulong)1 << 63))){
        out[temp[i]] = in[i];
        for(int p = 1; p < numOfOcc[(int)in[i]]; p++){
            out[temp[i] + p] = in[i + p];
        }
    }*/
    if(flag[i] == 1)
        out[temp[i]] = in[i];
}