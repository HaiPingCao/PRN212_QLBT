namespace QLBT.Shared.Models
{
    public enum Command
    {
        // Auth
        LOGIN,
        LOGIN_RESPONSE,

        // Assignment (Student view)
        GET_ASSIGNMENT_LIST,
        GET_ASSIGNMENT_DETAIL,

        // Download problem file (Server → Student)
        DOWNLOAD_PROBLEM_FILE,

        // Submit assignment - Upload (Student → Server)
        SUBMIT_ASSIGNMENT,
        SUBMIT_READY,
        FILE_CHUNK,
        FILE_END,
        SUBMIT_OK,

        // View / delete submission
        GET_SUBMISSION,
        GET_SUBMISSION_BY_ASSIGNMENT,
        DELETE_SUBMISSION,

        // Download submission (Server → Student)
        DOWNLOAD_SUBMISSION,

        // Download chunks (Server → Student)
        FILE_CHUNK_DOWN,
        FILE_END_DOWN,
    }
}