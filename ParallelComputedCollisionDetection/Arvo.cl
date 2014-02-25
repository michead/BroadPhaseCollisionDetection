#define XSHIFT 0
#define YSHIFT 4
#define ZSHIFT 8

#define ICType1 1
#define ICType2 2
#define ICType3 4
#define ICType4 8
#define ICType5 16
#define ICType6 32
#define ICType7 64
#define ICType8 128

/*#define R 16
#define GROUPS_PER_BLOCKS 12*/

#define HASH_FUNCTION(x, y, z, k) \
                    obj_array[i].cellIDs[k] = \
                        ((((uint)((x + 10) / ge)) << XSHIFT) |  \
                        (((uint)((y + 10) / ge)) << YSHIFT) |  \
                        (((uint)((z + 10) / ge)) << ZSHIFT)) + (uint)1;
                    
/*#define MOD(x, y) \
            (x - (y * (int)(x/y)))
*/

#define CELL_TYPE_CHECK(posX, posY, posZ) \
            posx = posX + 10;\
            posy = -(posY - 10);\
            posz = -(posZ - 10);\
            if (fmod(posx, dge) <= (ge) && fmod(posy, dge) <= (ge) && fmod(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType1;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (1 << 8);\
            }\
            else if (fmod(posx, dge) > (ge) && fmod(posy, dge) <= (ge) && fmod(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType2;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (2 << 8);\
            }\
            else if (fmod(posx, dge) <= (ge) && fmod(posy, dge) > (ge) && fmod(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType3;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (3 << 8);\
            }\
            else if (fmod(posx, dge) > (ge) && fmod(posy, dge) > (ge) && fmod(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType4;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (4 << 8);\
            }\
            else if (fmod(posx, dge) <= (ge) && fmod(posy, dge) <= (ge) && fmod(posz, dge) > (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType5;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (5 << 8);\
            }\
            else if (fmod(posx, dge) > (ge) && fmod(posy, dge) <= (ge) && fmod(posz, dge) > (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType6;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (6 << 8);\
            }\
            else if (fmod(posx, dge) <= (ge) && fmod(posy, dge) > (ge) && fmod(posz, dge) > (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType7;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (7 << 8);\
            }\
            else\
            {\
                obj_array[i].ctrl_bits |= ICType8;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (8 << 8);\
            }
            
#define CHECK_FOR_SPHERE_BOX_INTERSECTION(x2, y2, z2, x3, y3, z3) \
            dist_squared = radius * radius;\
            if (pos[0] < (x2)) dist_squared -= (pos[0] - (x2))*(pos[0] - (x2));\
            else if (pos[0] > (x3)) dist_squared -= (pos[0] - (x3))*(pos[0] - (x3));\
            if (pos[1] < (y2)) dist_squared -= (pos[1] - (y2))*(pos[1] - (y2));\
            else if (pos[1] > (y3)) dist_squared -= (pos[1] - (y3))*(pos[1] - (y3));\
            if (pos[2] < (z2)) dist_squared -= (pos[2] - (z2))*(pos[2] - (z2));\
            else if (pos[2] > (z3)) dist_squared -= (pos[2] - (z3))*(pos[2] - (z3));\
            res = dist_squared > 0;           

typedef struct{
    uint ID;
    uint ctrl_bits;
    float radius;
    float pos[3];
    uint cellIDs[8];
}BodyData;

__kernel void Arvo(__global BodyData* obj_array, __global const int* n, __global const float* grid_edge, __global uint* cellArray){

										
    int i = get_global_id(0);
    int id = get_local_id(0);
    int group = get_group_id(0);
    if(i>=*n) return;
            
    float cellPos[3];
    float pos[3];
    float posx;
    float posy;
    float posz;
    
    for(int g = 0; g < 8; g++)
        obj_array[i].cellIDs[g] = 0xFFFFFFFF;
            
    __local int radixCounter[256];
            
    float radius = obj_array[i].radius;
    float dist_squared = radius * radius;
            
    float ge = *grid_edge;
    float dge = 2 * ge;
            
    //pos = (float3)(obj_array[i].pos[0], obj_array[i].pos[1], obj_array[i].pos[2]);
    pos[0] = obj_array[i].pos[0];
    pos[1] = obj_array[i].pos[1];
    pos[2] = obj_array[i].pos[2];
            
    if(pos[0]>=0.0)
        cellPos[0] = ((int)((pos[0] + (ge) * 0.5) / (ge))) * (ge);
    else
        cellPos[0] = ((int)((pos[0] - (ge) * 0.5) / (ge))) * (ge);
    if (pos[1] >= 0.0)
        cellPos[1] = ((int)((pos[1] + (ge) * 0.5) / (ge))) * (ge);
    else
        cellPos[1] = ((int)((pos[1] - (ge) * 0.5) / (ge))) * (ge);
    if (pos[2] >= 0.0)
        cellPos[2] = ((int)((pos[2] + (ge) * 0.5) / (ge))) * (ge);
    else
        cellPos[2] = ((int)((pos[2] - (ge) * 0.5) / (ge))) * (ge);

    //hCell
    obj_array[i].cellIDs[0] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2], 0);
    CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2]);
            
    int j = 1;
    bool res;
            
        //COLLISION CHECK - (3^3 - 1) cases
            
        //right 1
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5, 
		cellPos[0] + ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2]);
	}

        //left 2
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2]);
	}

        //top 3
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
		cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2]);
	}

  //bottom 4
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
		cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2]);
	}

	//near 5
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] + ge);
	}

	//far 6
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] - ge);
	}

	//bottom_left 7
  CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2]);
	}
			
	//bottom_left_near 8
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] + ge);
	}


	//bottom_left_far 9
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
		cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] - ge);
	}

  //bottom_right 10
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
			cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2]);
	}

	//bottom_right_near 11
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
		cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] + ge);
	}

	//bottom_right_far 12
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] - ge);
	}

	//top_left 13
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2]);
	}

	//top_left_near 14
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] + ge);
	}

	//top_left_far 15
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] - ge);
	}

	//top_right 16
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
		cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2], j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2]);
	}

	//top_right_near 17
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] + ge);
	}

	//top_right_far 18
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] - ge);
	}


	//top_near 19
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2] + ge);
	}

	//bottom_near 20
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
		cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2] + ge);
	}

	//top_far 21
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2] - ge);
	}

	//bottom_far 22
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2] - ge);
	}

	//left_far 23
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2] - ge);
	}


	//right_far 24
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
		cellPos[0] + ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2] - ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2] - ge);
	}

  //left_near 25
	CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2] + ge);
	}
            
  //right_near 26
  CHECK_FOR_SPHERE_BOX_INTERSECTION
		(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
		cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 1.5);
	if(res){
		obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2] + ge, j);
		j++;
		CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2] + ge);
	}

	barrier(CLK_LOCAL_MEM_FENCE);
  



	
	//RADIX SORT
                        
	//PHASE 1
                        
  for(int l = 0; l < 8; l++)
		cellArray[i * 8 + l] = obj_array[i].cellIDs[l];
/*                        
  barrier(CLK_LOCAL_MEM_FENCE);
                        
  uint masked1;// = obj_array[i / 8].cellIDs[i % 8] & 255;
  uint masked2;// = obj_array[i / 8].cellIDs[i % 8] & 65280;
  uint masked3;// = obj_array[i / 8].cellIDs[i % 8] & 16711680;
  uint masked4;// = obj_array[i / 8].cellIDs[i % 8] & 4278190080;

	

	__local uint arrayPhase2_1[24];

	__local uint arrayPhase2_2[128];
                        
  int index = (group * GROUPS_PER_BLOCKS + id/R) * 103 + (id%R);
  while(index < 103 * (GROUPS_PER_BLOCKS*group + (id/R))){
                            
		masked1 = cellArray[index] & 255;
                        
		if(id % R == 0)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 1)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 2)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 3)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 4)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 5)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 6)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 7)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 8)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 9)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 10)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 11)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 12)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 13)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 14)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        

		if(id % R == 15)
			radixCounter[masked1]++;
		barrier(CLK_LOCAL_MEM_FENCE);
                        
		index += R;
	}

	

	//PHASE 2

	

	if(group % R == 0){

		for(int f = 0; f < 256; f++)		

			support[f + group * GROUPS_PER_BLOCKS + id] = radixCounter[f];

	}

	

	barrier(CLK_LOCAL_MEM_FENCE);

	

	//Reading Num_Groups counters of each radix from global memory to shared memory

	for(int m=0; m < 128; m++){

		if(id < 24)

			arrayPhase2_1[id] = radixCounter

		

		//PREFIX SUM

		

		

		

	}
   */                         
}