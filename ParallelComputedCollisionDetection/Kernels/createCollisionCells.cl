__kernel void createCollisionCells(     //__global const ulong* in, // 1024
                                        //__global ulong* out, // n
                                        __global uint* temp, //1024
                                        //__global uint* n, // 1
                                        //__global uint* offset, // 1093
                                        __global uint* iteration){ // 1093
    int i = get_global_id(0);
    
     //prefix sum
    //for(int j = 1; j <= (int)log2((float)*n); j++){
        uint temp2;
        uint temp3;
        barrier(CLK_GLOBAL_MEM_FENCE);
        uint iter = *iteration;
        barrier(CLK_GLOBAL_MEM_FENCE);
        if(i >= ((int)pow(2, (float)iter)))
            temp2 = temp[i - (int)pow(2, (float)(iter))];
            //barrier(CLK_GLOBAL_MEM_FENCE);
            //temp[i] = temp[i - (int)pow(2, (float)(j-1))] + temp[i];
        barrier(CLK_GLOBAL_MEM_FENCE);
        if(i >= ((int)pow(2, (float)iter)))
            temp3 = temp[i];
        barrier(CLK_GLOBAL_MEM_FENCE);
        if(i >= ((int)pow(2, (float)iter)))
            temp[i] = temp2 + temp3;
    //}
    
    /*
    
    barrier(CLK_GLOBAL_MEM_FENCE);
    
    if((i==0 || in[i]!=in[i-1]) && (in[i] & ((ulong)1 << 63))){
        out[temp[i]] = in[i];
        for(int p = 1; p < numOfOcc[(int)in[i]]; p++){
            out[temp[i] + p] = in[i + p];
        }
    }*/ 
}