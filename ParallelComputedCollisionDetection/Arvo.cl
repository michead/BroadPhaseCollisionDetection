#define HASH_FUNCTION \
                    ((int)(pos[0] / (*grid_edge)) << 0) |  \
                    ((int)(pos[1] / (*grid_edge)) << 3) |  \
                    ((int)(pos[2] / (*grid_edge)) << 6);   
                    
#define CELL_TYPE_CHECK(posX, posY, posZ) \
            posX += 10;\
            posY = -(posY - 10);\
            posZ = -(posZ - 10);\
            //case 1\
            if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) <= grid_edge)\
            {\
                cTypesIntersected |= 1;\
            }\
            //case 2\
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) <= grid_edge)\
            {\
                cTypesIntersected |= 2;\
            }\
            //case 3\
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) <= grid_edge)\
            {\
                cTypesIntersected |= 4;\
            }\
            //case 4\
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) <= grid_edge)\
            {\
                cTypesIntersected |= 8;\
            }\
            //case 5\
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) > grid_edge)\
            {\
                cTypesIntersected |= 16;\
            }\
            //case 6\
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) > grid_edge)\
            {\
                cTypesIntersected |= 32;\
            }\
            //case 7\
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) > grid_edge)\
            {\
                cTypesIntersected |= 64;\
            }\
            //case 8\
            else\
            {\
                cTypesIntersected |= 128;\
            }\
            //HomeCell recognition still to be implemented!
            

typedef struct{
    int ID;
    int control_bits;
    int cellIDs[8];
    double radius;
    float pos[3];
}ObjectProperties;

__kernel void Arvo(__global ObjectProperties* array, __global int* n, __global double* grid_edge){
            float cellPos[3];
            int i = get_global_id(0);
            if(i>=*n) return;
            float pos[3];
            pos[0] = array[i].pos[0];
            pos[1] = array[i].pos[1];
            pos[2] = array[i].pos[2];
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
            array[i].cellIDs[0] =   HASH_FUNCTION;                   
            

            int count = 0;
            int j = 1;
            
            //TODO COLLISION CHECK - 3^3 cases
            
}