__kernel void reorder(  __global uint* index, 
                        __global ulong* in, 
                        __global ulong* out){
    
    int i = get_global_id(0);

    out[i] = in[index[i]];
}