local stream = KEYS[1]
local keepN  = tonumber(ARGV[1]) or 0
local approx = ARGV[2]
if approx ~= "~" then approx = nil end

local groups = redis.call('XINFO', 'GROUPS', stream)
if (not groups) or (#groups == 0) then
if keepN > 0 then
  if approx then
    return redis.call('XTRIM', stream, 'MAXLEN', approx, keepN)
  else
    return redis.call('XTRIM', stream, 'MAXLEN', keepN)
  end
end
return 0
end


local function row_to_map(t)
local m = {}
for i = 1, #t, 2 do
  m[t[i]] = t[i+1]
end
return m
end


local minGuardId = nil
for i = 1, #groups do
local g = row_to_map(groups[i])
local gname   = g['name']
local pending = tonumber(g['pending']) or 0

if pending > 0 then

  local p = redis.call('XPENDING', stream, gname)
  if p and (#p >= 2) and p[2] then
    local id = p[2]
    if id and id ~= false then
      if (minGuardId == nil) or (id < minGuardId) then
        minGuardId = id
      end
    end
  end
else
  local last = g['last-delivered-id']
  if last and last ~= '' then
    if (minGuardId == nil) or (last < minGuardId) then
      minGuardId = last
    end
  end
end
end


if minGuardId == nil then
if keepN > 0 then
  if approx then
    return redis.call('XTRIM', stream, 'MAXLEN', approx, keepN)
  else
    return redis.call('XTRIM', stream, 'MAXLEN', keepN)
  end
end
return 0
end


if keepN > 0 then
local rev = redis.call('XREVRANGE', stream, '+', '-', 'COUNT', keepN)
if rev and (#rev > 0) then
  local nthFromEndId = rev[#rev][1]

  if nthFromEndId < minGuardId then
    minGuardId = nthFromEndId
  end
end
end


if approx then
return redis.call('XTRIM', stream, 'MINID', approx, minGuardId)
else
return redis.call('XTRIM', stream, 'MINID', minGuardId)
end