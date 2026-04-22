class AppError(Exception):
    status = 500; code = "INTERNAL_ERROR"; message = "Unexpected server error"
    def __init__(self, message=None, status=None, code=None, details=None):
        super().__init__(message or self.message)
        if status: self.status = status
        if code: self.code = code
        self.details = details or {}

class BadRequest(AppError): status=400; code="BAD_REQUEST"; message="Invalid request"
class Unauthorized(AppError): status=401; code="UNAUTHORIZED"; message="Authentication required"
class Forbidden(AppError): status=403; code="FORBIDDEN"; message="Forbidden"
class NotFound(AppError): status=404; code="NOT_FOUND"; message="Resource not found"
class Conflict(AppError): status=409; code="CONFLICT"; message="Conflict"
class Unprocessable(AppError): status=422; code="VALIDATION_ERROR"; message="Validation error"
