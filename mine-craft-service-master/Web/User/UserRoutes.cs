using MineCraftService.GameDataBase;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

public static class UserRoutes
{
    public static IEndpointRouteBuilder MapUserRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/user");

        // 登录
        group.MapGet("/", async (string username, string password, UserService userService) =>
        {
            var user = await userService.GetUserId(username, password);
            return user == null
                ? ApiResponse.Fail(401,"用户名或密码错误")
                : ApiResponse.Success(user);
        });

        // 注册
        group.MapPost("/", (User user, UserService userService) =>
            MyValidation.ValidateAndRunAsync(user,
                async _ =>
                {
                    if (user.UserName.Length is > 16 or < 4 || user.PassWord.Length is > 16 or < 4)
                        return ApiResponse.Fail(401,
                            "用户名或密码必须在4到16个字符之间");
                    var ok = await userService.CreateUser(user);
                    return ok
                        ? ApiResponse.Success(user)
                        : ApiResponse.Fail(401, "用户名已存在");
                }));

        return routes;
    }
}