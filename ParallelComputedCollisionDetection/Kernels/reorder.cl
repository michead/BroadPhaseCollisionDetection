__kernel void reorder(__global const uint* index, __global const ulong* in, __global ulong* out, __global int* n){
    int i = get_global_id(0);
    if(i >= *n)
        return;
    out[i] = in[index[i]];
}