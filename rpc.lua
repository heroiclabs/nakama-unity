local nk = require("nakama")

local M = {}

function M.loopback(context, payload)
  return payload
end
nk.register_rpc(M.loopback, "loopback")

print("Module loaded.")

return M
