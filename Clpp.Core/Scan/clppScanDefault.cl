#pragma OPENCL EXTENSION cl_amd_printf : enable

//#define T int

__kernel 
void kernel__ExclusivePrefixScanSmall(
	__global T* input,
	__global T* output,
	__local  T* block,
	const uint length)
{
	int tid = get_local_id(0);
	
	int offset = 1;
	block[2*tid]     = input[2*tid];
	block[2*tid + 1] = input[2*tid + 1];	
	for(int d = length>>1; d > 0; d >>=1)
	{
		barrier(CLK_LOCAL_MEM_FENCE);
		
		if(tid<d)
		{
			int ai = offset*(2*tid + 1) - 1;
			int bi = offset*(2*tid + 2) - 1;
			
			block[bi] += block[ai];
		}
		offset *= 2;
	}
	if(tid == 0)
		block[length - 1] = 0;
	for(int d = 1; d < length ; d *= 2)
	{
		offset >>=1;
		barrier(CLK_LOCAL_MEM_FENCE);
		
		if(tid < d)
		{
			int ai = offset*(2*tid + 1) - 1;
			int bi = offset*(2*tid + 2) - 1;
			
			float t = block[ai];
			block[ai] = block[bi];
			block[bi] += t;
		}
	}
	
	barrier(CLK_LOCAL_MEM_FENCE);
	output[2*tid]     = block[2*tid];
	output[2*tid + 1] = block[2*tid + 1];
}
#define NUM_BANKS 16
#define LOG_NUM_BANKS 4
#ifdef ZERO_BANK_CONFLICTS
#define CONFLICT_FREE_OFFSET(index) ((index) >> LOG_NUM_BANKS + (index) >> (2*LOG_NUM_BANKS))
#else
#define CONFLICT_FREE_OFFSET(index) ((index) >> LOG_NUM_BANKS)
#endif
__kernel
void kernel__ExclusivePrefixScan(
	__global T* dataSet,
	
	__local T* localBuffer,
	
	__global T* blockSums,
	const uint blockSumsSize
	)
{
	const uint gid = get_global_id(0);
	const uint tid = get_local_id(0);
	const uint bid = get_group_id(0);
	const uint lwz  = get_local_size(0);
	
	// The local buffer has 2x the size of the local-work-size, because we manage 2 scans at a time.
const uint localBufferSize = lwz << 1;
int offset = 1;
	
const int tid2_0 = tid << 1;
const int tid2_1 = tid2_0 + 1;
	
	const int gid2_0 = gid << 1;
const int gid2_1 = gid2_0 + 1;
	// Cache the datas in local memory
#ifdef SUPPORT_AVOID_BANK_CONFLICT
	uint ai = tid;
	uint bi = tid + lwz;
	uint gai = gid;
	uint gbi = gid + lwz;
	uint bankOffsetA = CONFLICT_FREE_OFFSET(ai); 
	uint bankOffsetB = CONFLICT_FREE_OFFSET(bi);
	localBuffer[ai + bankOffsetA] = (gai < blockSumsSize) ? dataSet[gai] : 0; 
	localBuffer[bi + bankOffsetB] = (gbi < blockSumsSize) ? dataSet[gbi] : 0;
#else
	localBuffer[tid2_0] = (gid2_0 < blockSumsSize) ? dataSet[gid2_0] : 0;
	localBuffer[tid2_1] = (gid2_1 < blockSumsSize) ? dataSet[gid2_1] : 0;
#endif
	
for(uint d = lwz; d > 0; d >>= 1)
	{
barrier(CLK_LOCAL_MEM_FENCE);
		
if (tid < d)
		{
#ifdef SUPPORT_AVOID_BANK_CONFLICT
			//uint ai = mad24(offset, (tid2_1+0), -1);	// offset*(tid2_0+1)-1 = offset*(tid2_1+0)-1
			uint i = 2 * offset * tid;
			uint ai = i + offset - 1;
			uint bi = ai + offset;
			ai += CONFLICT_FREE_OFFSET(ai);	// ai += ai / NUM_BANKS;
			bi += CONFLICT_FREE_OFFSET(bi);	// bi += bi / NUM_BANKS;
#else
const uint ai = mad24(offset, (tid2_1+0), -1);	// offset*(tid2_0+1)-1 = offset*(tid2_1+0)-1
const uint bi = mad24(offset, (tid2_1+1), -1);	// offset*(tid2_1+1)-1;
#endif
localBuffer[bi] += localBuffer[ai];
}
offset <<= 1;
}
barrier(CLK_LOCAL_MEM_FENCE);
	
	/*
	if (tid < 1)
		blockSums[bid] = localBuffer[localBufferSize-1];
		
	barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	
	if (tid < 1)
		localBuffer[localBufferSize - 1] = 0;
	*/
	
if (tid < 1)
	{
#ifdef SUPPORT_AVOID_BANK_CONFLICT
		uint index = localBufferSize-1;
		index += CONFLICT_FREE_OFFSET(index);
		blockSums[bid] = localBuffer[index];
		localBuffer[index] = 0;
#else
		// We store the biggest value (the last) to the sum-block for later use.
blockSums[bid] = localBuffer[localBufferSize-1];		
		//barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);		
		// Clear the last element
localBuffer[localBufferSize - 1] = 0;
#endif
}
for(uint d = 1; d < localBufferSize; d <<= 1)
	{
offset >>= 1;
barrier(CLK_LOCAL_MEM_FENCE);
		
if (tid < d)
		{
#ifdef SUPPORT_AVOID_BANK_CONFLICT
			//uint ai = mad24(offset, (tid2_1+0), -1);	// offset*(tid2_0+1)-1 = offset*(tid2_1+0)-1
			uint i = 2 * offset * tid;
			uint ai = i + offset - 1;
			uint bi = ai + offset;
			ai += CONFLICT_FREE_OFFSET(ai);	// Apply an offset to the __local memory
			bi += CONFLICT_FREE_OFFSET(bi);
#else
const uint ai = mad24(offset, (tid2_1+0), -1); // offset*(tid2_0+1)-1 = offset*(tid2_1+0)-1
const uint bi = mad24(offset, (tid2_1+1), -1); // offset*(tid2_1+1)-1;
#endif
T tmp = localBuffer[ai];
localBuffer[ai] = localBuffer[bi];
localBuffer[bi] += tmp;
}
}
barrier(CLK_LOCAL_MEM_FENCE);
	
#ifdef SUPPORT_AVOID_BANK_CONFLICT
	dataSet[gai] = (gai < blockSumsSize) * localBuffer[ai + bankOffsetA];		
	dataSet[gbi] = (gbi < blockSumsSize) * localBuffer[bi + bankOffsetB];		
#else
	if (gid2_0 < blockSumsSize)
		dataSet[gid2_0] = localBuffer[tid2_0];
	if (gid2_1 < blockSumsSize)
		dataSet[gid2_1] = localBuffer[tid2_1];
#endif
}
__kernel
void kernel__UniformAdd(
	__global T* output,
	__global const T* blockSums,
	const uint outputSize
	)
{
uint gid = get_global_id(0) * 2;
const uint tid = get_local_id(0);
const uint blockId = get_group_id(0);
	
	// Intel SDK fix
	//output[gid] += blockSums[blockId];
	//output[gid+1] += blockSums[blockId];
__local T localBuffer[1];
#ifdef SUPPORT_AVOID_BANK_CONFLICT
	uint blockOffset = 1024 - 1;
if (tid < 1)
localBuffer[0] = blockSums[blockId + blockOffset];
#else
if (tid < 1)
localBuffer[0] = blockSums[blockId];
#endif
barrier(CLK_LOCAL_MEM_FENCE);
	
#ifdef SUPPORT_AVOID_BANK_CONFLICT
	unsigned int address = blockId * get_local_size(0) * 2 + get_local_id(0); 
	
	output[address] += localBuffer[0];
output[address + get_local_size(0)] += (get_local_id(0) + get_local_size(0) < outputSize) * localBuffer[0];
#else
	if (gid < outputSize)
		output[gid] += localBuffer[0];
	gid++;
	if (gid < outputSize)
		output[gid] += localBuffer[0];
#endif
}
;
