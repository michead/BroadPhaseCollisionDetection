#define HASH_FUNCTION(x, y, z) \
                    ((int)(x / (*grid_edge)) << 0) |  \
                    ((int)(y / (*grid_edge)) << 3) |  \
                    ((int)(z / (*grid_edge)) << 6)
                    
#define MOD(x, y) \
            x - (y * (int)x/(int)y)
                    
#define CELL_TYPE_CHECK(posX, posY, posZ) \
            posx = posX + 10;\
            posy = -(posY - 10);\
            posz = -(posZ - 10);\
            if (MOD(posx, dge) <= (*grid_edge) && MOD(posy, dge) <= (*grid_edge) && MOD(posz, dge) <= (*grid_edge))\
            {\
                obj_array->control_bits |= 1;\
            }\
            else if (MOD(posx, dge) > (*grid_edge) && MOD(posy, dge) <= (*grid_edge) && MOD(posz, dge) <= (*grid_edge))\
            {\
                obj_array->control_bits |= 2;\
            }\
            else if (MOD(posx, dge) <= (*grid_edge) && MOD(posy, dge) > (*grid_edge) && MOD(posz, dge) <= (*grid_edge))\
            {\
                obj_array->control_bits |= 4;\
            }\
            else if (MOD(posx, dge) > (*grid_edge) && MOD(posy, dge) > (*grid_edge) && MOD(posz, dge) <= (*grid_edge))\
            {\
                obj_array->control_bits |= 8;\
            }\
            else if (MOD(posx, dge) <= (*grid_edge) && MOD(posy, dge) <= (*grid_edge) && MOD(posz, dge) > (*grid_edge))\
            {\
                obj_array->control_bits |= 16;\
            }\
            else if (MOD(posx, dge) > (*grid_edge) && MOD(posy, dge) <= (*grid_edge) && MOD(posz, dge) > (*grid_edge))\
            {\
                obj_array->control_bits |= 32;\
            }\
            else if (MOD(posx, dge) <= (*grid_edge) && MOD(posy, dge) > (*grid_edge) && MOD(posz, dge) > (*grid_edge))\
            {\
                obj_array->control_bits |= 64;\
            }\
            else\
            {\
                obj_array->control_bits |= 128;\
            }
            
#define CHECK_FOR_SPHERE_BOX_INTERSECTION(x2, y2, z2, x3, y3, z3) \
            if (pos[0] < x2) dist_squared -= (float)(pos[0] - x2)*(pos[0] - x2);\
            else if (pos[0] > x3) dist_squared -= (float)(pos[0] - x3)*(pos[0] - x3);\
            if (pos[1] < y2) dist_squared -= (float)(pos[1] - y2)*(pos[1] - y2);\
            else if (pos[1] > y3) dist_squared -= (float)(pos[1] - y3)*(pos[1] - y3);\
            if (pos[2] < z2) dist_squared -= (float)(pos[2] - z2)*(pos[2] - z2);\
            else if (pos[2] > z3) dist_squared -= (float)(pos[2] - z3)*(pos[2] - z3);\
            if (dist_squared > 0)\
                res = 1;\
            else\
                res = 0;            

typedef struct{
    int ID;
    int control_bits;
    int cellIDs[8];
    double radius;
    float pos[3];
}ObjectProperties;

__kernel void Arvo(__global ObjectProperties* obj_array, __global int* n, __global double* grid_edge){
            float cellPos[3];
            int i = get_global_id(0);
            if(i>=*n) return;
            float pos[3];
            pos[0] = obj_array[i].pos[0];
            pos[1] = obj_array[i].pos[1];
            pos[2] = obj_array[i].pos[2];
            if(pos[0]>=0)
                cellPos[0] = ((int)((pos[0] + (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);
            else
                cellPos[0] = ((int)((pos[0] - (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);
            if (pos[1] >= 0)
                cellPos[1] = ((int)((pos[1] + (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);
            else
                cellPos[1] = ((int)((pos[1] - (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);
            if (pos[2] >= 0)
                cellPos[2] = ((int)((pos[2] + (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);
            else
                cellPos[2] = ((int)((pos[2] - (*grid_edge) * 0.5f) / (*grid_edge))) * (*grid_edge);

            //hCell
            obj_array[i].cellIDs[0] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2]);
            
            int j = 1;
            int res;
            
            double radius = obj_array->radius;
            float dist_squared = radius * radius;
            
            float posx;
            float posy;
            float posz;
            double dge = 2 * (*grid_edge);
            
            //COLLISION CHECK - (3^3 - 1) cases
            
            //right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1], cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1], cellPos[2]);
			}

            //left
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1], cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1], cellPos[2]);
			}

            //top
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + *grid_edge, cellPos[2]);
			}

            //bottom
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - *grid_edge, cellPos[2]);
			}

            //near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] + *grid_edge);
			}

            //far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1], cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1], cellPos[2] - *grid_edge);
			}

            //bottom_left
            CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2]);
			}
			
            //bottom_left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
			}

            //bottom_left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
			}

            //bottom_right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2]);
			}

            //bottom_right_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
			}

            //bottom_right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
			}

            //top_left
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2]);
			}

            //top_left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
			}

            //top_left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
			}

            //top_right
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2]);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2]);
			}

            //top_right_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
			}

            //top_right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
			}

            //top_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + *grid_edge, cellPos[2] + *grid_edge);
			}

            //bottom_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - *grid_edge, cellPos[2] + *grid_edge);
			}

            //top_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] + *grid_edge * 1.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] + *grid_edge, cellPos[2] - *grid_edge);
			}

            //bottom_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 0.5f, cellPos[1] - *grid_edge * 1.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0], cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0], cellPos[1] - *grid_edge, cellPos[2] - *grid_edge);
			}

            //left_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1], cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1], cellPos[2] - *grid_edge);
			}

            //right_far
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] - *grid_edge * 1.5f, 
				cellPos[0] + *grid_edge * 1.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] - *grid_edge * 0.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1], cellPos[2] - *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1], cellPos[2] - *grid_edge);
			}

            //left_near
			CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] - *grid_edge * 1.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] - *grid_edge, cellPos[1], cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] - *grid_edge, cellPos[1], cellPos[2] + *grid_edge);
			}
            
            //right_near
            CHECK_FOR_SPHERE_BOX_INTERSECTION
				(cellPos[0] + *grid_edge * 0.5f, cellPos[1] - *grid_edge * 0.5f, cellPos[2] + *grid_edge * 0.5f, 
				cellPos[0] - *grid_edge * 0.5f, cellPos[1] + *grid_edge * 0.5f, cellPos[2] + *grid_edge * 1.5f);
			if(res == 1){
				obj_array[i].cellIDs[j] = HASH_FUNCTION(cellPos[0] + *grid_edge, cellPos[1], cellPos[2] + *grid_edge);
				j++;
				CELL_TYPE_CHECK(cellPos[0] + *grid_edge, cellPos[1], cellPos[2] + *grid_edge);
			}    
}