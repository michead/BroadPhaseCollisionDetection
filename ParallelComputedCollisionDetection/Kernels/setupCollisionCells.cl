//#define ITERATIONS 10

__kernel void setupCollisionCells(__global const ulong* in, //1024
                                   __global ulong* out, //1024
                                   __global uint* temp, //1024
                                   __global uint* n, // 1
                                   __global uint* offset, // 1093
                                   __global uint* numOfOcc){ // 1093
    int i = get_global_id(0);
    
    //if((uint)in[i] == (uint)4294967295)
        //return;
    
    if((uint)in[i] != (uint)4294967295)
        atom_inc(&numOfOcc[(uint)in[i]]);
    
    barrier(CLK_GLOBAL_MEM_FENCE);
    
    //count of occurrences of each cell ID
    if((i==0 || ((uint)in[i] != (uint)in[i-1]))/* && (in[i] & ((ulong)1 << 63))*/ && ((uint)in[i] != (uint)4294967295)){
        //offset[(uint)in[i]] = (uint)i;
        //offset[(uint)in[i]] |= ((uint)(1 << 16));
        temp[i] = 1;
        
        //temp[i] = numOfOcc[(uint)in[i]]; //!!!!!!  why copy doesn't work???? -- barrier works?
        for(int p = 1; p < numOfOcc[(uint)in[i]]; p++){
           temp[i + p] = 1;
        }
        //atom_xchg(numOfCCells, *numOfCCells + numOfOcc[(int)in[i]]);
    }
    
    //barrier(CLK_GLOBAL_MEM_FENCE);
    /*
    //prefix sum
    for(int j = 1; j <= (int)log2((float)*n); j++){
        if(i >= (int)pow(2, (float)j)){
            barrier(CLK_GLOBAL_MEM_FENCE);
            uint temp2 = temp[i - (int)pow(2, (float)(j-1))];
            barrier(CLK_GLOBAL_MEM_FENCE);
            atom_xchg(&temp[i], temp2 + temp[i]);
            temp[i] = temp[i - (int)pow(2, (float)(j-1))] + temp[i];
        }
        barrier(CLK_GLOBAL_MEM_FENCE);
    }
    
    if((uint)in[i] == 0xFFFFFFFF)
        temp[i] = 0;
    
    barrier(CLK_GLOBAL_MEM_FENCE);
    
    if((i==0 || in[i]!=in[i-1]) && (in[i] & ((ulong)1 << 63))){
        out[temp[i]] = in[i];
        for(int p = 1; p < numOfOcc[(int)in[i]]; p++){
            out[temp[i] + p] = in[i + p];
        }
    }
    barrier(CLK_GLOBAL_MEM_FENCE);*/
}