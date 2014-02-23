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

#define HASH_FUNCTION(x, y, z, k) \
                    obj_array[i].cellIDs[k] = \
                        (((uint)((x + 10) / ge)) << XSHIFT) |  \
                        (((uint)((y + 10) / ge)) << YSHIFT) |  \
                        (((uint)((z + 10) / ge)) << ZSHIFT) + 1;
                    
#define MOD(x, y) \
            (x - (y * (int)(x/y)))
                    
#define CELL_TYPE_CHECK(posX, posY, posZ) \
            posx = posX + 10;\
            posy = -(posY - 10);\
            posz = -(posZ - 10);\
            if (MOD(posx, dge) <= (ge) && MOD(posy, dge) <= (ge) && MOD(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType1;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (1 << 8);\
            }\
            else if (MOD(posx, dge) > (ge) && MOD(posy, dge) <= (ge) && MOD(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType2;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (2 << 8);\
            }\
            else if (MOD(posx, dge) <= (ge) && MOD(posy, dge) > (ge) && MOD(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType3;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (3 << 8);\
            }\
            else if (MOD(posx, dge) > (ge) && MOD(posy, dge) > (ge) && MOD(posz, dge) <= (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType4;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (4 << 8);\
            }\
            else if (MOD(posx, dge) <= (ge) && MOD(posy, dge) <= (ge) && MOD(posz, dge) > (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType5;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (5 << 8);\
            }\
            else if (MOD(posx, dge) > (ge) && MOD(posy, dge) <= (ge) && MOD(posz, dge) > (ge))\
            {\
                obj_array[i].ctrl_bits |= ICType6;\
                if(posX == cellPos[0] && posY == cellPos[1] && posZ == cellPos[2])\
                    obj_array[i].ctrl_bits |= (6 << 8);\
            }\
            else if (MOD(posx, dge) <= (ge) && MOD(posy, dge) > (ge) && MOD(posz, dge) > (ge))\
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
            if (pos[0] < x2) dist_squared -= (pos[0] - x2)*(pos[0] - x2);\
            else if (pos[0] > x3) dist_squared -= (pos[0] - x3)*(pos[0] - x3);\
            if (pos[1] < y2) dist_squared -= (pos[1] - y2)*(pos[1] - y2);\
            else if (pos[1] > y3) dist_squared -= (pos[1] - y3)*(pos[1] - y3);\
            if (pos[2] < z2) dist_squared -= (pos[2] - z2)*(pos[2] - z2);\
            else if (pos[2] > z3) dist_squared -= (pos[2] - z3)*(pos[2] - z3);\
            if (dist_squared > 0)\
                res = 1;\
            else\
                res = 0;            

typedef struct{
    uint ID;
    uint ctrl_bits;
    float radius;
    float pos[3];
    uint cellIDs[8];
}BodyData;

__kernel void Arvo(__global BodyData* obj_array, __global const int* n, __global const float* grid_edge){
            int i = get_global_id(0);
            if(i>=*n) return;
            
            float cellPos[3];
            float pos[3];
            float posx;
            float posy;
            float posz;
            
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
            int res;
            
            //COLLISION CHECK - (3^3 - 1) cases
            
            //right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5, 
				cellPos[0] + ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2]);
			}

            //left
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2]);
			}

            //top
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
				cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2]);
			}

            //bottom
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
				cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2]);
			}

            //near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] + ge);
			}

            //far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] - ge);
			}

            //bottom_left
            CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2]);
			}
			
            //bottom_left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] + ge);
			}

            //bottom_left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
				cellPos[0] - ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] - ge, cellPos[2] - ge);
			}

            //bottom_right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 0.5, 
				cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2]);
			}

            //bottom_right_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
				cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] + ge);
			}

            //bottom_right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] - ge, cellPos[2] - ge);
			}

            //top_left
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2]);
			}

            //top_left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] + ge);
			}

            //top_left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1] + ge, cellPos[2] - ge);
			}

            //top_right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5, 
				cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2], j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2]);
			}

            //top_right_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] + ge);
			}

            //top_right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 1.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1] + ge, cellPos[2] - ge);
			}

            //top_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2] + ge);
			}

            //bottom_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] + ge * 0.5, 
				cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2] + ge);
			}

            //top_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 0.5, cellPos[1] + ge * 1.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + ge, cellPos[2] - ge);
			}

            //bottom_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 0.5, cellPos[1] - ge * 1.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - ge, cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - ge, cellPos[2] - ge);
			}

            //left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2] - ge);
			}

            //right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] - ge * 1.5, 
				cellPos[0] + ge * 1.5, cellPos[1] + ge * 0.5, cellPos[2] - ge * 0.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2] - ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2] - ge);
			}

            //left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - ge * 1.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - ge, cellPos[1], cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - ge, cellPos[1], cellPos[2] + ge);
			}
            
            //right_near
            CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + ge * 0.5, cellPos[1] - ge * 0.5, cellPos[2] + ge * 0.5, 
				cellPos[0] - ge * 0.5, cellPos[1] + ge * 0.5, cellPos[2] + ge * 1.5);
			if(res == 1 &&  j < 8){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + ge, cellPos[1], cellPos[2] + ge, j);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + ge, cellPos[1], cellPos[2] + ge);
			}
}