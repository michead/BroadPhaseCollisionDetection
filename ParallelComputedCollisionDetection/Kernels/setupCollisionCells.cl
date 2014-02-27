//#define ITERATIONS 10

__kernel void setupCollisionCells(__global const ulong* in, //1024
                                   __global ulong* out, //1024
                                   __global uint* temp, //1024
                                   __global uint* n, // 1
                                   __global uint* offset, // 1093
                                   __global uint* numOfOcc){ // 1093
    int i = get_global_id(0);
    
    if(i >= *n || ((in[i] == 0xFFFFFFFF)))
        return;
        
    //atom_xchg(&temp[i], i);
    //uint nOO = numOfOcc[(uint)in[i]];
    //numOfOcc[in[i]] = numOfOcc[in[i]] + 1;
    //atom_xchg(&numOfOcc[(uint)in[i]], nOO + 1);
    //atom_inc(&temp[i]);
    //count of occurrences of each cell ID
    if((i==0 || in[i] != in[i-1])/* && (in[i] & ((ulong)1 << 63))*/){
        //temp[i] = numOfOcc[in[i]];
        //offset[(uint)in[i]] = (uint)i;
        //true collision cell
        //offset[(uint)in[i]] |= ((uint)1 << 16);
        //atom_xchg(&temp[i], 1);
        //atom_xchg(&temp[i], numOfOcc[(uint)in[i]]);
        //for(int p = 1; p < numOfOcc[(int)in[i]]; p++){
            //atom_xchg(&temp[i + p], 1);
            //offset[(uint)in[i + p]] |= ((uint)1 << 16);
        //}
        //atom_xchg(numOfCCells, *numOfCCells + numOfOcc[(int)in[i]]);
    }
    
    barrier(CLK_LOCAL_MEM_FENCE);
    /*
    //prefix sum
    for(int j = 1; j <= (int)log2((float)*n); j++){
        if(i >= (int)pow(2, (float)j)){
            barrier(CLK_LOCAL_MEM_FENCE);
            uint temp2 = temp[i - (int)pow(2, (float)(j-1))];
            barrier(CLK_LOCAL_MEM_FENCE);
            atom_xchg(&temp[i], temp2 + temp[i]);
            temp[i] = temp[i - (int)pow(2, (float)(j-1))] + temp[i];
        }
        barrier(CLK_LOCAL_MEM_FENCE);
    }
    
    if((uint)in[i] == 0xFFFFFFFF)
        temp[i] = 0;
    
    barrier(CLK_LOCAL_MEM_FENCE);
    
    if((i==0 || in[i]!=in[i-1]) && (in[i] & ((ulong)1 << 63))){
        out[temp[i]] = in[i];
        for(int p = 1; p < numOfOcc[(int)in[i]]; p++){
            out[temp[i] + p] = in[i + p];
        }
    }
    barrier(CLK_LOCAL_MEM_FENCE);*/
}